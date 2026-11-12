# Non-gameplay navigation consistency

The user requested Close and Back to match the current coin shop's glossy orange/red
button family. Work stays local: no upload, commit, push, authentication, merge or deployment.

Preserve finalized Home and Settings, accepted source art, gameplay, saves and callback
semantics. Do not globally reskin Modal because gameplay goal/help surfaces also use it.
Existing coin-shop Close and forest Back assets have been inspected before implementation;
reuse them byte-for-byte. There is no new or redrawn bitmap. The Back keeps its white arrow
rather than substituting a Close symbol. The asset manifest records original dimensions and hashes.

An explicit MenuNavigation helper standardizes actual game-space size to the current coin
shop's 116 x 119 control at its 1920/1717 canvas scale. Canvas-local callers pass that scale;
ordinary 1080 x 1920 callers use the default. Input semantics, press tint and original art are
retained. Modal callers opt in individually and retain the previous Close center.

Ownership: the integration owner handles MenuNavigation and non-gameplay page navigation;
shop pages, purchase dialogs, draw results and the seven other dialog functions remain owned
by their isolated workers. Integrate function-level changes without replacing shared files.

Validate source/asset hashes, geometry, input and return behavior in both languages. Verify
finalized Home/Settings and gameplay sources did not change. Run the aggregate suite and real
GUI after combined integration; headless tests alone do not establish visual acceptance.

Final combined review also covers the preview-only chapter pagination labels. Reuse the
existing painted words on equal product surfaces instead of requesting a missing brown
Next label. The map current marker must remain below the level card: use local sibling
order, not a positive z-index that crosses the modal layer. Neither fix changes rules.
