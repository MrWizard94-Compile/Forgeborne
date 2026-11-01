"""Forgeborne original procedural sound palette; standard library only, no sampled material.

Outputs deterministic mono 48 kHz PCM WAVs and a bounded signal/identity manifest.
These are authored candidates. Signal checks do not establish mix quality or fun.
"""
from array import array
from hashlib import sha256
from pathlib import Path
import json
import math
import random
import struct
import wave

RATE = 48000
ROOT = Path(__file__).resolve().parents[1]
DEST = ROOT / "ForgeborneUnity" / "Assets" / "Resources" / "Audio"
TAU = math.tau


def envelope(t, length, attack=0.004, decay=12.0):
    return min(1.0, t / attack) * math.exp(-decay * t / length) * min(1.0, (length - t) / 0.012)


def synth(name, length, seed, voice):
    rng = random.Random(seed)
    noise = 0.0
    values = []
    for i in range(round(length * RATE)):
        t = i / RATE
        raw = rng.uniform(-1, 1)
        noise = 0.76 * noise + 0.24 * raw
        values.append(voice(t, length, raw, noise))
    peak = max(abs(x) for x in values)
    gain = 0.62 / peak if peak else 0
    pcm = array("h", (round(max(-0.62, min(0.62, x * gain)) * 32767) for x in values))
    # array byte order is native; WAV is always little endian.
    if struct.pack("=H", 1) != struct.pack("<H", 1):
        pcm.byteswap()
    path = DEST / (name + ".wav")
    with wave.open(str(path), "wb") as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(RATE)
        wav.writeframes(pcm.tobytes())
    with wave.open(str(path), "rb") as wav:
        assert wav.getnframes() == len(values) and wav.getframerate() == RATE
    assert pcm[0] == 0 and abs(pcm[-1]) < 20
    return {"file": path.name, "seconds": len(values) / RATE, "sampleRate": RATE,
            "peak": max(abs(x) for x in pcm) / 32768,
            "rms": math.sqrt(sum((x / 32768) ** 2 for x in pcm) / len(pcm)),
            "sha256": sha256(path.read_bytes()).hexdigest()}


def main():
    DEST.mkdir(parents=True, exist_ok=True)
    sounds = []
    def add(name, length, seed, voice):
        sounds.append(synth(name, length, seed, voice))

    add("PulseDischarge", 0.19, 4201, lambda t, l, n, b: envelope(t,l,0.0018,7) *
        (0.7*math.sin(TAU*(145*t + 39*(1-math.exp(-45*t)))) + 0.22*n*math.exp(-70*t) + 0.18*b))
    add("StoneChisel", 0.15, 4202, lambda t,l,n,b: envelope(t,l,0.001,6) *
        (0.5*n*math.exp(-30*t)+0.22*math.sin(TAU*1760*t)+0.16*math.sin(TAU*2917*t)))
    add("ConstructionLatch", 0.25, 4203, lambda t,l,n,b: envelope(t,l,0.003,5) *
        (0.6*math.sin(TAU*92*t)+0.3*b+0.12*math.sin(TAU*705*t)*math.exp(-28*t)))
    add("MagazineLatch", 0.30, 4204, lambda t,l,n,b:
        envelope(t,l,0.002,5)*(0.5*b+0.2*math.sin(TAU*700*t)) +
        (envelope(t-0.12,l-0.12,0.002,7)*(0.4*n+0.35*math.sin(TAU*170*t)) if t>0.12 else 0))
    add("Impact", 0.11, 4205, lambda t,l,n,b: envelope(t,l,0.001,7) *
        (0.6*n*math.exp(-40*t)+0.35*math.sin(TAU*245*t)))
    add("SuitImpact", 0.19, 4206, lambda t,l,n,b: envelope(t,l,0.003,6) *
        (0.8*math.sin(TAU*(63*t+1.3*(1-math.exp(-20*t))))+0.3*b))
    add("ActionRefused", 0.12, 4207, lambda t,l,n,b: envelope(t,l,0.006,4) *
        (math.sin(TAU*180*t)+0.18*math.sin(TAU*361*t)))
    add("Footfall", 0.16, 4208, lambda t,l,n,b: envelope(t,l,0.003,8) *
        (0.65*b+0.6*math.sin(TAU*76*t)))
    add("SuitBreach", 0.70, 4209, lambda t,l,n,b: envelope(t,l,0.014,3) *
        (math.sin(TAU*(210*t-105*t*t))+0.28*math.sin(TAU*(420*t-210*t*t))+0.1*b))
    add("RelayCollapse", 0.85, 4210, lambda t,l,n,b: envelope(t,l,0.008,4) *
        (0.6*math.sin(TAU*(100*t-38*t*t))+0.45*b+0.15*math.sin(TAU*47*t)))
    # Integer cycles and a periodic envelope produce a seamless eight-second bed.
    add("FoundryBed", 8.0, 4211, lambda t,l,n,b: math.sin(math.pi*t/l)**2 *
        (0.4*math.sin(TAU*48*t)+0.18*math.sin(TAU*96.125*t)+
         0.10*math.sin(TAU*144*t)*(0.8+0.2*math.sin(TAU*0.5*t))))
    manifest = {"generator": "tools/generate-original-audio.py", "original": True,
                "source": "Original synthesis from oscillators, seeded noise and envelopes; no recordings or external assets",
                "acceptance": "Signal/format checks only; audible/native mix review remains required", "sounds": sounds}
    output = ROOT / "docs" / "evidence" / "2026-11-01" / "audio-signals.json"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(f"Generated and checked {len(sounds)} original WAVs at {RATE} Hz; mix acceptance pending.")


if __name__ == "__main__":
    main()
