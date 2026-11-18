"""Apply the localized launcher title to the project produced by Creator."""
import plistlib
import sys
from pathlib import Path

platform = sys.argv[1]
engine = Path(__file__).resolve().parents[2] / "PerchSort/native/engine" / platform
names = {"en": "Fly Home", "zh": "枝头小分队"}

if platform == "ios":
    plist = engine / "Info.plist"
    info = plistlib.loads(plist.read_bytes())
    info["CFBundleDevelopmentRegion"] = "en"
    info["CFBundleDisplayName"] = names["en"]
    plist.write_bytes(plistlib.dumps(info, sort_keys=False))
    for locale, title in names.items():
        folder = engine / f"{locale}.lproj"
        folder.mkdir(exist_ok=True)
        (folder / "InfoPlist.strings").write_text(f'"CFBundleDisplayName" = "{title}";\n', encoding="utf-8")
elif platform == "android":
    # Creator owns app_name. Keep the launcher's translations in a distinct string resource.
    manifest = engine / "app/AndroidManifest.xml"
    manifest.write_text(manifest.read_text().replace('android:label="@string/app_name"',
                                                      'android:label="@string/launcher_title"'))
    for qualifier, title in [("values", names["en"]), ("values-zh", names["zh"])]:
        folder = engine / "res" / qualifier
        folder.mkdir(parents=True, exist_ok=True)
        (folder / "launcher_title.xml").write_text(
            f'<resources><string name="launcher_title">{title}</string></resources>\n', encoding="utf-8")
else:
    raise SystemExit(f"Unknown native platform: {platform}")
