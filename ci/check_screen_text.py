#!/usr/bin/env python3
"""Keep screen text in one place: the game font through game/Flat.ts.

Screen words are drawn in the game font by game/Flat.ts (Flat.text), the only place allowed to create a Label; the
transparent EditBox label in Popups.tool_purchase is input plumbing. Every other Label/RichText site is rejected.
Static word references are inspected with TypeScript's parser.
"""
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT / "PerchSort"
RESOURCES = GAME / "assets" / "resources"
SCRIPTS = GAME / "assets" / "scripts"


ANALYZE = r"""
const fs = require('node:fs');
const path = require('node:path');
const ts = require(process.argv[1]);
const root = process.argv[2];
const findings = [], refs = [];
function strings(node) {
  if (!node) return null;
  if (ts.isStringLiteralLike(node)) return [node.text];
  if (ts.isParenthesizedExpression(node) || ts.isAsExpression(node)) return strings(node.expression);
  if (ts.isConditionalExpression(node)) {
    const a=strings(node.whenTrue),b=strings(node.whenFalse);
    return a && b ? [...a,...b] : null;
  }
  if (ts.isBinaryExpression(node) && node.operatorToken.kind===ts.SyntaxKind.PlusToken) {
    const a=strings(node.left),b=strings(node.right);
    return a && b ? a.flatMap(x=>b.map(y=>x+y)) : null;
  }
  return null;
}
function inspect(file) {
  const text=fs.readFileSync(file,'utf8');
  const source=ts.createSourceFile(file,text,ts.ScriptTarget.Latest,true,ts.ScriptKind.TS);
  const aliases=new Map();
  for(const statement of source.statements) if(ts.isImportDeclaration(statement) && statement.moduleSpecifier.text==='cc') {
    const bindings=statement.importClause?.namedBindings;
    if(bindings && ts.isNamedImports(bindings)) for(const el of bindings.elements) aliases.set(el.name.text,(el.propertyName??el.name).text);
  }
  const location=n=>`${path.relative(root,file)}:${source.getLineAndCharacterOfPosition(n.getStart(source)).line+1}`;
  function transparentQuantity(node) {
    let method=node;
    while(method&&!ts.isMethodDeclaration(method)) method=method.parent;
    if(!method || method.name.getText(source)!=='tool_purchase' || path.basename(file)!=='Popups.ts') return false;
    const assignment=node.parent;
    if(!ts.isVariableDeclaration(assignment)) return false;
    const variable=assignment.name.getText(source),writes=[]; let attached=false,hasText=false;
    function scan(n) {
      if(ts.isBinaryExpression(n) && n.operatorToken.kind===ts.SyntaxKind.EqualsToken) {
        if(ts.isPropertyAccessExpression(n.left) && n.left.expression.getText(source)===variable) {
          if(n.left.name.text==='color') writes.push(n.right);
          if(n.left.name.text==='string') hasText=true;
        }
        if(ts.isPropertyAccessExpression(n.left) && n.left.name.text==='textLabel' && n.right.getText(source)===variable) attached=true;
      }
      ts.forEachChild(n,scan);
    }
    scan(method.body);
    const color=writes[0];
    return attached&&!hasText&&writes.length===1&&ts.isNewExpression(color)&&aliases.get(color.expression.getText(source))==='Color'&&color.arguments?.length===4&&color.arguments[3].getText(source)==='0';
  }
  function visit(node) {
    if(ts.isCallExpression(node) && ts.isPropertyAccessExpression(node.expression)) {
      const owner=node.expression.expression.getText(source),member=node.expression.name.text;
      if(member==='addComponent' && node.arguments.length && ['Label','RichText'].includes(aliases.get(node.arguments[0].getText(source)))) {
        if(!file.endsWith(path.join('game','Flat.ts'))&&(aliases.get(node.arguments[0].getText(source))!=='Label'||!transparentQuantity(node))) findings.push(location(node)+': a font component outside Flat.text is forbidden');
      }
      if(['fillText','strokeText'].includes(member)) findings.push(location(node)+': canvas font rendering is forbidden');
      let kind=null,index=1;
      if(owner==='UI'&&member==='word')kind='word';
      if(owner==='UI'&&member==='painted')kind='painted';
      if(owner==='Kit'&&member==='word')kind='either';
      if(owner==='Kit'&&['lettering','sign','jungle','purchase'].includes(member))kind='lettering';
      if(owner==='Kit'&&member==='page'){kind='lettering';index=2;}
      if(kind){const values=strings(node.arguments[index]);if(values)for(const value of values)if(value)refs.push({at:location(node),kind,value});}
    }
    if(ts.isNewExpression(node)&&['Label','RichText'].includes(aliases.get(node.expression.getText(source))))findings.push(location(node)+': font component construction is forbidden');
    if(ts.isBinaryExpression(node)&&ts.isPropertyAccessExpression(node.left)&&['innerText','textContent'].includes(node.left.name.text))findings.push(location(node)+': DOM text is not painted game UI');
    ts.forEachChild(node,visit);
  }
  visit(source);
}
function walk(dir){for(const entry of fs.readdirSync(dir,{withFileTypes:true})){const file=path.join(dir,entry.name);if(entry.isDirectory())walk(file);else if(file.endsWith('.ts'))inspect(file);}}
walk(path.join(root,'PerchSort/assets/scripts'));
process.stdout.write(JSON.stringify({findings,refs}));
"""


def check_source():
    module = GAME / "node_modules" / "typescript"
    if not module.is_dir():
        raise RuntimeError("TypeScript parser unavailable; run npm --prefix PerchSort ci first")
    output = subprocess.check_output(["node", "-e", ANALYZE, str(module), str(ROOT)], text=True)
    result = json.loads(output)
    problems = result["findings"]
    for path in sorted((GAME / "assets").rglob("*")):
        if path.suffix not in {".scene", ".prefab"}:
            continue
        nodes = json.loads(path.read_text(encoding="utf-8"))
        for node in nodes:
            if node.get("__type__") in {"cc.Label", "cc.RichText"}:
                problems.append(f"{path.relative_to(ROOT)}: serialized visible font component is forbidden")
    return problems, len(result["refs"])


def main():
    if sys.argv[1:]:
        print("Usage: python3 ci/check_screen_text.py (no ratchet update mode is needed)", file=sys.stderr)
        return 2
    try:
        problems, refs = check_source()
    except (OSError, ValueError, KeyError, RuntimeError, subprocess.CalledProcessError) as error:
        print(f"Painted words: inspection failed: {error}", file=sys.stderr)
        return 2
    for problem in problems:
        print(problem, file=sys.stderr)
    print(f"Screen text: {refs} word references, {len(problems)} problem(s).")
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
