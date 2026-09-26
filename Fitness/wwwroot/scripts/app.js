window.downloadFile = (filename, content) => {
  const blob = new Blob([content], { type: 'text/csv' });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
};

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
    const context = getAudioContext();
    if (!context)
      return;
    if (context.state !== 'running')
      context.resume();
    const now = context.currentTime;
    for (let i = 0; i < 3; i++)
      beep(context, now + i * 0.25);
    if (navigator.vibrate)
      navigator.vibrate([200, 50, 200, 50, 200]);
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
