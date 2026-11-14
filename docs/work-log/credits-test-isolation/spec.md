# Credits fixture isolation

Follow-up #254: the credits-only UI regression should use the same muted synthetic Profile as neighboring gameplay-dialog tests. Set `muted` and `music_muted` on that local fixture before serialization. Do not alter production Sound, user settings, UI, callbacks, or any audio behavior. This is not an audio leak repair or proof of leak-free production playback.
