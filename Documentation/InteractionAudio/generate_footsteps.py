"""Regenerate only the three quiet shoe-friction cues; preserve asset GUIDs."""
from pathlib import Path
import json
import wave
import numpy as np

ROOT=Path(__file__).resolve().parents[2]
RATE=44100
rng=np.random.default_rng(20261007)
report=[]
for name,seconds,low,high in [
    ('FootStone',.30,90,500),
    ('FootWood',.32,70,420),
    ('FootSnow',.38,110,600),
]:
    count=round(seconds*RATE)
    frequencies=np.fft.rfftfreq(count,1/RATE)
    # Broad noise, without resonant strikes or bright hiss.
    spectrum=np.fft.rfft(rng.normal(size=count))
    spectrum*=np.exp(-(frequencies/high)**4)*(1-np.exp(-(frequencies/low)**4))
    sound=np.fft.irfft(spectrum,n=count)
    sound*=np.sin(np.linspace(0,np.pi,count))**2
    sound*=min(.03/np.sqrt(np.mean(sound**2)),.16/np.max(np.abs(sound)))
    pcm=np.round(sound*32767).astype('<i2')
    path=ROOT/'Assets/ReportVR/Audio'/(name+'.wav')
    with wave.open(str(path),'wb') as file:
        file.setnchannels(1);file.setsampwidth(2);file.setframerate(RATE)
        file.writeframes(pcm.tobytes())
    decoded=pcm.astype(float)/32767
    energy=np.abs(np.fft.rfft(decoded))**2
    high_energy=energy[frequencies>=1000].sum()/energy.sum()
    peak=np.max(np.abs(decoded));rms=np.sqrt(np.mean(decoded**2))
    assert .01<rms<=.031 and peak<=.161 and high_energy<1e-5
    assert max(abs(decoded[0]),abs(decoded[-1]))<.001
    report.append(f'PASS {name}: {seconds:.2f}s, peak={peak:.4f}, RMS={rms:.4f}, energy above 1kHz={high_energy:.2e}; smooth friction, no strike tone.')

manifest=ROOT/'Assets/ReportVR/Audio/audio-manifest.json'
rows=json.loads(manifest.read_text(encoding='utf-8'))
durations={'FootStone':.30,'FootWood':.32,'FootSnow':.38}
for row in rows:
    if row['cue'] in durations:row['seconds']=durations[row['cue']]
manifest.write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8')
(Path(__file__).parent/'footsteps-waveform.txt').write_text('\n'.join(report)+'\n',encoding='utf-8')
print('\n'.join(report))
