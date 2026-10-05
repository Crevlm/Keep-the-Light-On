mergeInto(LibraryManager.library, {
  KtloPlaytestInit: function (urlPtr, keyPtr) {
    if (window.ktloPlaytest) return;
    var endpoint = UTF8ToString(urlPtr), key = UTF8ToString(keyPtr);
    var storageKey = 'ktlo-playtest-pending-v1';
    var state = { queue: [], last: null, lastAt: 0, busy: false, retry: 1000, timer: null, failures: 0 };
    function persist() {
      try { sessionStorage.setItem(storageKey, JSON.stringify({ endpoint: endpoint, at: Date.now(), events: state.queue })); } catch (_) {}
    }
    try {
      var saved = JSON.parse(sessionStorage.getItem(storageKey) || 'null');
      if (saved && saved.endpoint === endpoint && Date.now() - saved.at < 86400000 && Array.isArray(saved.events)) state.queue = saved.events.slice(-500);
    } catch (_) {}
    function pack() {
      var events = state.queue.slice(0, 40);
      return { events: events, body: JSON.stringify({ collectionKey: key, events: events }) };
    }
    function schedule(ms) {
      if (state.timer) return;
      state.timer = setTimeout(function () { state.timer = null; flush(); }, ms);
    }
    function flush() {
      if (state.busy || !state.queue.length) return;
      state.busy = true;
      var batch = pack();
      fetch(endpoint, { method: 'POST', headers: { 'Content-Type': 'text/plain;charset=UTF-8' }, body: batch.body, credentials: 'omit', signal: AbortSignal.timeout(15000) })
        .then(function (res) { if (!res.ok) throw new Error('upload ' + res.status); return res.json(); })
        .then(function (ack) {
          if (!ack.accepted) throw new Error('missing acknowledgement');
          var ids = new Set(batch.events.map(function (e) { return e.eventId; }));
          state.queue = state.queue.filter(function (e) { return !ids.has(e.eventId); });
          state.retry = 1000; state.failures = 0; persist();
        })
        .catch(function () { state.retry = Math.min(30000, state.retry * 2); state.failures++; })
        .finally(function () { state.busy = false; if (state.queue.length) schedule(state.retry); });
    }
    function push(event) {
      event.visible = event.visible && document.visibilityState === 'visible';
      state.last = event; state.lastAt = Date.now();
      state.queue.push(event);
      if (state.queue.length > 500) {
        var heartbeat = state.queue.findIndex(function (e) { return e.type === 'heartbeat'; });
        state.queue.splice(heartbeat >= 0 ? heartbeat : 0, 1);
        console.warn('Playtest retry buffer is full; oldest buffered evidence was dropped.');
      }
      persist(); schedule(300);
    }
    function lifecycle(type) {
      if (!state.last || state.last.type === 'session_end') return;
      var now = Date.now(), e = Object.assign({}, state.last, {
        eventId: crypto.randomUUID().replace(/-/g, ''), sequence: state.last.sequence + 0.01,
        elapsedMs: state.last.elapsedMs + Math.min(60000, now - state.lastAt), utc: new Date(now).toISOString(),
        type: type, node: '', detail: '', visible: document.visibilityState === 'visible'
      });
      push(e);
      if (document.visibilityState === 'hidden' || type === 'page_hidden') {
        var batch = pack();
        // Beacon is best effort. Keep records until a later fetch acknowledges them; server deduplicates IDs.
        if (navigator.sendBeacon) navigator.sendBeacon(endpoint, new Blob([batch.body], { type: 'text/plain;charset=UTF-8' }));
      } else flush();
    }
    document.addEventListener('visibilitychange', function () { lifecycle(document.visibilityState === 'hidden' ? 'tab_hidden' : 'tab_visible'); });
    window.addEventListener('pagehide', function () {
      lifecycle('page_hidden');
      if (state.last && state.last.type === 'session_end' && state.queue.length && navigator.sendBeacon)
        navigator.sendBeacon(endpoint, new Blob([pack().body], { type: 'text/plain;charset=UTF-8' }));
    });
    window.addEventListener('pageshow', function () { lifecycle('page_visible'); });
    window.addEventListener('online', function () { state.retry = 1000; flush(); });
    window.ktloPlaytest = { push: push, flush: flush, pending: function () { return state.queue.length; } };
    if (state.queue.length) schedule(300);
  },
  KtloPlaytestEvent: function (jsonPtr) {
    if (window.ktloPlaytest) window.ktloPlaytest.push(JSON.parse(UTF8ToString(jsonPtr)));
  }
});
