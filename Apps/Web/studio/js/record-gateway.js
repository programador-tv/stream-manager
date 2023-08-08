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
    config.Instance.send(
      rawData
    );
  };

  const endpointTransmit = wsurl + '/transmit';
  config.Instance = new WebSocket(endpointTransmit);
onmessage = function(e) {  
    const buffer = e.data;
    chunks.push(buffer);
    
    
    let file = new File(chunks,'filename',{type:'video/webm'});
  
    reader.readAsArrayBuffer(file);

    chunks.length = 0;
  };

  