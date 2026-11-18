# Platform packaging

Adapt the iOS, Android and WeChat Mini Game packaging commands from crazyniu/fishsort (main commit 63b2a5b). Use the Perch Sort scene UUID, portrait settings and existing art. Keep production identities, hosting and signing external to the repository. Preserve the existing web and editor launch paths.

The public packaging entry is an executable package.sh with a numbered platform menu. Selecting iOS or Android opens an artifact menu. Returning or exiting must not start a build.
