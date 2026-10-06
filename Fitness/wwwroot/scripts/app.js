window.downloadFile = (filename, content) => {
  const blob = new Blob([content], { type: 'text/csv' });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
};

window.copyToClipboard = async (text) => {
  const fallbackCopy = () => {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.setAttribute('readonly', '');
    textArea.style.cssText = 'position:fixed;top:0;left:0;opacity:0;font-size:16px;';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();
    textArea.setSelectionRange(0, text.length);
    let isCopied = false;
    try {
      isCopied = document.execCommand('copy');
    } catch {
      isCopied = false;
    }
    textArea.remove();
    window.getSelection()?.removeAllRanges();
    return isCopied;
  };

  if (navigator.clipboard && window.isSecureContext) {
    try {
      await navigator.clipboard.writeText(text);
      return true;
    } catch {
      return fallbackCopy();
    }
  }
  return fallbackCopy();
};

document.addEventListener('mousedown', (event) => {
  if (event.target instanceof Element && event.target.closest('.mud-overlay-dialog'))
    event.preventDefault();
}, true);

const clearUndoHistory = () => {
  const input = document.createElement('input');
  input.type = 'text';
  input.tabIndex = -1;
  input.setAttribute('aria-hidden', 'true');
  input.style.cssText = 'position:fixed;left:-9999px;top:0;opacity:0;pointer-events:none;';
  document.body.appendChild(input);
  input.value = ' ';
  input.value = '';
  input.remove();
};

document.addEventListener('focusout', (event) => {
  if (event.target instanceof HTMLInputElement || event.target instanceof HTMLTextAreaElement)
    setTimeout(clearUndoHistory, 0);
}, true);

document.addEventListener('visibilitychange', () => {
  if (document.visibilityState === 'hidden')
    clearUndoHistory();
});

window.restTimer = (() => {
  let audioContext = null;
  let alarmInterval = null;
  let alarmTimeout = null;
  let wakeLock = null;
  let keepAwake = false;

  const getAudioContext = () => {
    if (!audioContext) {
      const AudioContextType = window.AudioContext || window.webkitAudioContext;
      if (!AudioContextType)
        return null;
      audioContext = new AudioContextType();
    }
    return audioContext;
  };

  const unlockAudio = () => {
    const context = getAudioContext();
    if (!context || context.state === 'running')
      return;
    context.resume();
    const source = context.createBufferSource();
    source.buffer = context.createBuffer(1, 1, 22050);
    source.connect(context.destination);
    source.start(0);
  };
  document.addEventListener('touchend', unlockAudio, true);
  document.addEventListener('click', unlockAudio, true);

  if (navigator.audioSession)
    navigator.audioSession.type = 'transient';

  const beep = (context, startTime) => {
    const oscillator = context.createOscillator();
    const gain = context.createGain();
    oscillator.type = 'square';
    oscillator.frequency.value = 880;
    gain.gain.setValueAtTime(0.0001, startTime);
    gain.gain.exponentialRampToValueAtTime(0.4, startTime + 0.01);
    gain.gain.exponentialRampToValueAtTime(0.0001, startTime + 0.18);
    oscillator.connect(gain);
    gain.connect(context.destination);
    oscillator.start(startTime);
    oscillator.stop(startTime + 0.2);
  };

  const playBeepPattern = () => {
    if (navigator.vibrate)
      navigator.vibrate([200, 50, 200, 50, 200]);
    const context = getAudioContext();
    if (!context)
      return;
    const play = () => {
      const now = context.currentTime;
      for (let i = 0; i < 3; i++)
        beep(context, now + i * 0.25);
    };
    if (context.state === 'running')
      play();
    else
      context.resume().then(play).catch(() => { });
  };

  const requestWakeLock = async () => {
    if (!keepAwake || wakeLock || !('wakeLock' in navigator) || document.visibilityState !== 'visible')
      return;
    try {
      wakeLock = await navigator.wakeLock.request('screen');
      wakeLock.addEventListener('release', () => { wakeLock = null; });
    } catch {
      wakeLock = null;
    }
  };

  document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible')
      requestWakeLock();
  });

  const stopAlarm = () => {
    clearInterval(alarmInterval);
    clearTimeout(alarmTimeout);
    alarmInterval = null;
    alarmTimeout = null;
  };

  return {
    startAlarm: (maxSeconds) => {
      stopAlarm();
      playBeepPattern();
      alarmInterval = setInterval(playBeepPattern, 1500);
      alarmTimeout = setTimeout(stopAlarm, maxSeconds * 1000);
    },
    stopAlarm,
    primeAudio: unlockAudio,
    setKeepAwake: (enabled) => {
      keepAwake = enabled;
      if (enabled) {
        requestWakeLock();
      } else if (wakeLock) {
        wakeLock.release();
        wakeLock = null;
      }
    }
  };
})();
