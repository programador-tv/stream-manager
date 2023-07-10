function getSupportedMimeType(types) {
    return types.find((type) => MediaRecorder.isTypeSupported(type)) || null;
}

export function startBroadcast(targetStream){
    let types = [
            'video/webm;codecs=h264',
            'video/webm',
            'video/mp4'
        ];

    let mimeType = getSupportedMimeType(types);
    if (null === mimeType) {
        alert("Navegador não suporta gravação de vídeo.");
        throw new Error("Navegador não suporta gravação de vídeo.");
    }
    const mediaRecorder = new MediaRecorder(targetStream, {
        mimeType: mimeType,
        videoBitsPerSecond: 2000000,
        audioBitsPerSecond: 128000,
        video: {
            width: 1280,
            height: 720,
            frameRate: 30,
        },
        audio: {
            codec: 'aac',
            bitrate: '128k',
            channels: 2,
            sampleRate: 44100,
        },
    });
    if (!mediaRecorder) {
        console.error("Failed to create MediaRecorder instance");
    }

    const videoWorker = new Worker('./js/record-gateway.js');

    mediaRecorder.ondataavailable = e => {
        videoWorker.postMessage(e.data);
    };
    mediaRecorder.start(4000);

    window.onbeforeunload = (e)=> {
        return  e.type;
    }   
}

