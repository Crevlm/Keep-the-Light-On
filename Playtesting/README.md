# Keep the Light On playtesting

The recorder is integrated into the existing Unity scripts. It creates itself automatically when PlaytestSettings is enabled; no scene objects or Inspector references need to be added.

## Use it

1. In Unity, open **Tools → Playtesting → Settings**.
2. Keep recording enabled and use a new build label for each version you distribute.
3. Leave **Record editor tests** off unless testing the collector from Play Mode. These sessions appear under **Editor tests**, separately from players.
4. Build WebGL and upload that new build to itch.io. An already-uploaded build cannot acquire this code automatically.
5. Open the dashboard using the button in the settings window. Sign in with the owning ChatGPT account.

Anonymous submissions are enabled. A live test verified itch.io CORS and a successful test upload. Session results and CSV exports still require the owner account; anonymous requests and forged identity headers were rejected. Rebuild and upload the instrumented WebGL game before collecting real player sessions.

## What is recorded

- New anonymous ID for every gameplay session/restart, plus a manually chosen build label.
- Accepted lamp relights, counts per room, EarlyRelight milestones, and narrative stage.
- The first-darkness response and persistent Early/Middle/Late path.
- Dialogue queued and dialogue started as separate events. Started is not proof that the player finished reading.
- Room entries, normalized light level, ten-second check-ins, visibility changes, restarts, and leaving gameplay.
- Last observed state and elapsed time. State 5 does not mean completion.

The existing first-darkness thresholds remain unchanged: stages 0–2 select Early, 3–5 select Middle, and 6–8 select Late. The recorder does not decide these paths.

## Read the results

Use Player sessions or Editor tests, then filter by narrative path. Click a session ID to inspect its timeline and estimated observed time in each room. Export summary CSV for the visible sessions or event CSV for one complete session.

The dashboard shows the most recent 500 sessions in each category. “No recent reports” means no upload for at least 45 seconds. It can mean leaving, switching tabs, loss of connection, browser suspension, or a disabled tracker. Do not interpret it as a confirmed quit. Room durations exclude hidden intervals and cap missing intervals at 15 seconds. Restarted sessions appear in the stopping-room summary with their explicit reason.

## Ending integration later

Call `PlaytestRecorder.CompleteGame()` only from the real ending when it is implemented. Nothing currently calls it. Simply reaching Room State 5 is recorded separately.

## Delivery and limits

The browser sends small batches without blocking the game. Failed requests retry with backoff. A temporary, tab-scoped buffer stores up to 500 pending events for up to 24 hours; when full, it drops a heartbeat first, otherwise the oldest event. Server IDs make retransmission safe. Page-hide delivery uses a best-effort beacon; sudden browser/process termination can still lose unsent events. Editor test sends do not use the browser retry buffer.

The collector accepts at most 40 events per batch, 64 KB per request, about 10,000 events per session, and 100,000 events per UTC day. These are lightweight abuse bounds, not a competitive-game anticheat system. Anyone inspecting the WebGL build can extract the write-only collection identifier and fabricate submissions. It cannot read results. No platform service credential is embedded in the game.

No names, email addresses, account IDs, device fingerprints, chat text, or precise locations are added to playtest events. Web hosting necessarily handles ordinary request metadata. A suggested itch.io notice: “This playtest sends anonymous gameplay events such as relights, room progression, and session timing to help improve the game.”

## Files and rollback

New files: Assets/Scripts/PlaytestRecorder.cs, Assets/Plugins/WebGL/PlaytestBridge.jslib, Assets/Resources/PlaytestSettings.json, Assets/Editor/PlaytestSettingsWindow.cs. Targeted hooks were added to NarrativeTextController, RoomController, and GameController. Original scripts are backed up in this task's playtest-staging/original-scripts folder. The user's pre-existing scene and font changes were not edited by the instrumentation patch.

Turn off **Enable recording** and rebuild to remove collection from the next distributed build.

