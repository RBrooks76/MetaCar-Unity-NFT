const io = require('socket.io')(8123, { //8123 is the local port we are binding the demo server to
  pingInterval: 30005,		//An interval how often a ping is sent
  pingTimeout: 5000,		//The time a client has to respont to a ping before it is desired dead
  upgradeTimeout: 3000,		//The time a client has to fullfill the upgrade
  allowUpgrades: true,		//Allows upgrading Long-Polling to websockets. This is strongly recommended for connecting for WebGL builds or other browserbased stuff and true is the default.
  cookie: false,			//We do not need a persistence cookie for the demo - If you are using a load balöance, you might need it.
  serveClient: true,		//This is not required for communication with our asset but we enable it for a web based testing tool. You can leave it enabled for example to connect your webbased service to the same server (this hosts a js file).
  allowEIO3: false,			//This is only for testing purpose. We do make sure, that we do not accidentially work with compat mode.
  cors: {
    origin: "*"				//Allow connection from any referrer (most likely this is what you will want for game clients - for WebGL the domain of your sebsite MIGHT also work)
  }
});


//This funciton is needed to let some time pass by between conversation and closing. This is only for demo purpose.
function sleep(ms) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}  

// App Code starts here

console.log('Starting Socket.IO demo server');

io.on('connection', (socket) => {
	console.log('[' + (new Date()).toUTCString() + '] game connecting');
	
    socket.on('KnockKnock', (data) => {
		console.log('[' + (new Date()).toUTCString() + '] game knocking... Answering "Who\'s there?"...');
        socket.emit('WhosThere');
    });

    socket.on('ItsMe', async (data) => {
		console.log('[' + (new Date()).toUTCString() + '] received game introduction. Welcoming the guest...');
        socket.emit('Welcome', 'Hi customer using unity' + data.version + ', this is the backend microservice. Thanks for buying our asset. (No data is stored on our server)');
        socket.emit('TechData', {
			podName: 'Local Test-Server',
			timestamp: (new Date()).toUTCString()
		});
    });
	
	socket.on('SendNumbers', async (data) => {
		console.log('[' + (new Date()).toUTCString() + '] Client is asking for random number array');
		socket.emit('RandomNumbers', [ Math.ceil((Math.random() * 100)), Math.ceil((Math.random() * 100)), Math.ceil((Math.random() * 100)) ]);
	});
	
	socket.on('Goodbye', async (data) => {
		console.log('[' + (new Date()).toUTCString() + '] Client said "' + data + '" - The server will disconnect the client in five seconds. You can now abort the process (and restart it afterwards) to see an auto reconnect attempt.');
		await sleep(5000); //This is only for demo purpose.
		socket.disconnect(true);
	});

	socket.on('disconnect', (data) => {
		console.log('[' + (new Date()).toUTCString() + '] Bye, client ' + socket.id);
	});


	
	socket.on('PING', async (data) => {
		console.log('[' + (new Date()).toUTCString() + '] incoming PING #' + data + ' from ' + socket.id + ' answering PONG with some jitter...');
		await sleep(Math.random() * 2000);
        socket.emit('PONG', data);
    });
	
});

