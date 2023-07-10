const wsurl = 'ws://localhost:5042';

const config = {
  started : false,
  Instance: null,
}

let actual = 0;
const chunks = []
const reader = new FileReader();
  reader.onload = function(e) {
    const rawData = e.target.result;
    config.socketTransmit.send(
      rawData
    );
  };

onmessage = function(e) {

    if(!config.started && !config.Instance) {
      config.started = true;
      const endpointTransmit = wsurl + '/transmit';
      config.Instance = new WebSocket(endpointTransmit);
    }
    
    const buffer = e.data;
    chunks.push(buffer);
    
    
    let file = new File(chunks,'filename',{type:'video/webm'});
  
    reader.readAsArrayBuffer(file);

    chunks.length = 0;
  };

  