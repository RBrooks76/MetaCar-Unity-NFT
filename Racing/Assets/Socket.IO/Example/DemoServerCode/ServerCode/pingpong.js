const io = require('socket.io')(8124, { //8124 is the local port we are binding the pingpong server to
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


//This funciton is needed to let some time pass by
function sleep(ms) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}  

// App Code starts here

console.log('Starting Socket.IO pingpong server');

io.on('connection', (socket) => {
	var cnt = 0;
	console.log('[' + (new Date()).toUTCString() + '] unity connecting with SocketID ' + socket.id);	
	
    socket.on('PING', (data) => {
		cnt++;
		console.log('[' + (new Date()).toUTCString() + '] incoming PING #' + data + ' answering PONG with some jitter...');
		//await sleep(10);
        socket.emit('PONG', data);
    });

	socket.on('disconnect', (reason) => {
		console.log('[' + (new Date()).toUTCString() + '] ' + socket.id + ' disconnected after ' + cnt + ' pings. Reason: ' + reason);
	});

});

