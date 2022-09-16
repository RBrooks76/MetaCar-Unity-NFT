var roomlist = [];
var database = null;
var io;
//var roommanager = require('../room_manager/roommanager');
var dateFormat = require("dateformat");
const { emit } = require('nodemon');
var socketlist = [];

exports.initdatabase = function (db) {
    database = db;
};
exports.addsocket = function (id)
{
    socketlist.push(id);
}
exports.setsocketio = function (socketio) {
    io = socketio;
};

exports.getroomlist = function () {
    return roomlist;
}

exports.addroom = function (r_roomID, r_title, r_creator, r_username, r_seatlimit, r_status, r_game_mode, r_wifi_mode, r_stake_money, r_win_money, socket) {
    let inputplayerlist = [];
    let inputnamelist = [];
    let inputbotFlaglist = [];
    let inputautoFlaglist = [];
    let playerphotos = [];
    let earnScore = [];
    let diceHistory = [];
    let gameobject = {
        roomid: r_roomID,
        title: r_title,
        creator: r_creator,
        username : r_username,
        seatlimit: parseInt(r_seatlimit),
        status: r_status,
        game_mode: r_game_mode,
        wifi_mode: r_wifi_mode,
        stake_money: r_stake_money,
        win_money: r_win_money,
        playerlist: inputplayerlist,
        namelist : inputnamelist,
        botFlaglist: inputbotFlaglist,
        autoFlaglist: inputautoFlaglist,
        playerphotos: playerphotos,
        earnScores: earnScore,
        dice: 1,
        turnuser: '',
        diceHistory: diceHistory,
        turncount: [],
        turnIndex: 1,
        move_history: {
            status : '',
            mover: '',
            path : ''
        },
    }
    roomlist.push(gameobject);
}

exports.GetRoomPassedTime = function (socket, data) {
    for (let index = 0; index < roomlist.length; index++) {
        if (roomlist[index].roomid == data.roomid) {
            roomlist[index].passedtime = parseFloat(data.passedtime);
        }
    }
}

exports.BotEnterRoom = function (data)
{
    let roomid = data.roomID;
    console.log('--- BotEnterRoom No:' + roomid + ' roomlist:' + roomlist.length);
    if (roomlist.length > 0) 
    {
        for (let index = 0; index < roomlist.length; index++)
        {
            if (roomlist[index].roomid == roomid) 
            {
                console.log('--- BotEnterRoom 2 ---');
                if (roomlist[index].playerlist.length == roomlist[index].seatlimit)
                {
                    //let mydata = {
                    //        result: "failed"
                    //    }
                    console.log('--- GameRoom is full players already ---');
                    //socket.emit('REQ_ENTER_ROOM_RESULT', mydata);
                    return;
                }
                else
                {
                    console.log('--- BotEnterRoom 3 ---');
                    let names = ['Aeira','Decuno','Harahe','Ailton','Guest268953','Cilina','Mereth',
                        'Hithach','Amanicus','Comhink'];

                    let needAddBotCount = roomlist[index].seatlimit - roomlist[index].playerlist.length;
                    for (let i = 0; i < needAddBotCount; i++) 
                    {
                        //let value = randomNum(0, names.length);
                        let name = 'Guest' + randomNum(24563, 958738).toString();
                        roomlist[index].playerlist.push((i + 1000).toString());
                        //roomlist[index].namelist.push((i + 1000).toString());
                        //roomlist[index].namelist.push(names[value]);
                        roomlist[index].namelist.push(name);
                        roomlist[index].botFlaglist.push(1);
                        roomlist[index].autoFlaglist.push(0);
                        roomlist[index].playerphotos.push('');
                        roomlist[index].earnScores.push(0);

                        //names.splice(value, 1);

                        console.log('--- Add Bot ---' + (i + 1000).toString());
                    } 

                    exports.GetUserListInRoom(roomid);
                }

                if (roomlist[index].playerlist.length == roomlist[index].seatlimit) 
                {
                    console.log('----- GameRoom Player=' + roomlist[index].playerlist)
                    // start game
                    roomlist[index].turnuser = roomlist[index].playerlist[0];
                    console.log('----- GameRoom is full players, so GAME START turnuser=' + roomlist[index].playerlist[0]);
                    let mydata = {
                        result: "success"
                    }
                    io.sockets.in('r' + roomid).emit('REQ_ENTER_ROOM_RESULT', mydata);
                    roomlist[index].status = "full";
                    UpdateRoomStatus(parseInt(roomid));
                }
            }
        }
    }
}

exports.playerenterroom = function (roomid, userid, username, photo, socket) {
    socket.room = 'r' + roomid;
    socket.userid = userid;
    //socket.nickname = username;
    console.log("----- player joined in room No: " + roomid + " roomlist:" + roomlist.length + " ------");
    socket.join('r' + roomid);
    
    if (roomlist.length > 0) {
        for (let index = 0; index < roomlist.length; index++) {
            if (roomlist[index].roomid == roomid) {
                //console.log("----- player joined in room1 ------")
                for (let i = 0; i < roomlist[index].playerlist.length; i++) 
                {
                    let uid = roomlist[index].playerlist[i];
                    if (uid == userid) {                        
                        let mydata = {
                            result: "failed"
                        }
                        console.log('--- userid ' + userid + ' joined already in room ---');
                        socket.emit('REQ_ENTER_ROOM_RESULT', mydata);
                        return;
                    }
                }

                if (roomlist[index].playerlist.length == roomlist[index].seatlimit)
                {
                    let mydata = {
                            result: "full"
                        }
                        console.log('--- GameRoom is full players ---');
                        socket.emit('REQ_ENTER_ROOM_RESULT', mydata);
                    return;
                }

                //console.log("----- player joined in room 2 ------")
                roomlist[index].playerlist.push(userid);
                roomlist[index].namelist.push(username);
                roomlist[index].botFlaglist.push(0);
                roomlist[index].autoFlaglist.push(0);
                roomlist[index].playerphotos.push(photo);
                roomlist[index].earnScores.push(0);

                //console.log("----- player joined in room 3 ------")
                exports.GetUserListInRoom(roomid);

                if (roomlist[index].playerlist.length == roomlist[index].seatlimit) {
                    // start game
                    roomlist[index].turnuser = userid;
                    console.log('----- GameRoom is full players, so GAME START -----');
                    let mydata = {
                        result: "success"
                    }
                    io.sockets.in('r' + roomid).emit('REQ_ENTER_ROOM_RESULT', mydata);
                    roomlist[index].status = "full";
                    UpdateRoomStatus(roomid);
                }
            }
        }
    }

    // roommanager.GetRoomList();
}
exports.reconnectRoom = function (roomid, username, userid, old_socketID, socket)
{
    console.log("reconnectRoom roomid", roomid, userid, username, old_socketID);

    let roomindex = -1;
    for (let index = 0; index < roomlist.length; index++) {
        if (roomlist[index].roomid == roomid) {
            roomindex = index;
        }
    }
    
    if (roomindex == -1)
    {
        let emitdata = {
            message: "exitRoom"
        }
        socket.emit('EXIT_GAME', emitdata);
        console.log("Room already got exit");
        return;
    }

    let ischeck = roomlist[roomindex].playerlist.filter(function (object) {
        return (object == userid)
    });

    if(ischeck.length == 0)
    {
        let emitdata = {
            message: "exitUser"
        }
        socket.emit('EXIT_GAME', emitdata);
        console.log("You already got disconnection");
    }
    else
    {
        for (let i = 0; i < roomlist[roomindex].namelist.length; i++)
        {
            if (roomlist[roomindex].namelist[i] == username)
            {
                roomlist[roomindex].botFlaglist[i] = 0;
                break;
            }
        }

        socketlist.splice(socketlist.indexOf(old_socketID),1);

        socketlist.push(socket.id);
        //console.log("reconn", roomid, username);
        socket.room = 'r' + roomid;
        socket.userid = userid;
        socket.username = username;
        socket.join('r' + roomid);
        let emit_data = {
            //roomid: roomid,
            reconnecter : userid,
            socketid: socket.id,
            status: roomlist[roomindex].move_history.status,
            mover: roomlist[roomindex].move_history.mover,
            path: roomlist[roomindex].move_history.path
        }
        io.sockets.in('r' + roomid).emit('RECONNECT_RESULT', emit_data);
    }
}
exports.GetUserListInRoom = async function (roomid) {
    //console.log('---REQ_USERLIST_ROOM_RESULT 0---  ');
    let roomindex = 0;
    let mydata = '';
    for (let index = 0; index < roomlist.length; index++) {
        if (roomlist[index].roomid == roomid) {
            roomindex = index;
        }
    }

    let winning_amounts = [];
    let two_playeds = [];
    let four_playeds = [];
    let win_streaks = [];
    let exps = [];
    let points = [];
    let levels = [];

    console.log('getUserInfo');

    var collection = database.collection('userdatas');
    for (let i = 0; i < roomlist[roomindex].playerlist.length; i++)
    {
        console.log('getUserInfo 1 ', roomlist[roomindex].playerlist[i]);
        var query = {userid : roomlist[roomindex].playerlist[i]};
        try {
            const res = await collection.findOne(query);
            if(res == null)
            {
                console.log('getUserInfo error');
                winning_amounts.push(29800);
                var two_play = {played : 32, won : 17};
                var four_play = {played : 14, won : 8};
                var won_streaks = {rate : 54, streak : 0};
                two_playeds.push(two_play);
                four_playeds.push(four_play);
                win_streaks.push(won_streaks);
                exps.push(10);
                points.push(43900);
                levels.push(6);
            }
            else
            {
                console.log('getUserInfo ', res);
                winning_amounts.push(res.winning_amount);
                two_playeds.push(res.two_play);
                four_playeds.push(res.four_play);
                win_streaks.push(res.won_streaks);
                exps.push(res.exp);
                points.push(res.points);
                levels.push(res.level);
            }
        } catch (e) {
            console.log(e);
        }
    }

    for (let i = 0; i < roomlist[roomindex].namelist.length; i++) 
    {
        /*
        mydata = mydata + '{' +
            '"userid":"' + roomlist[roomindex].playerlist[i] + '",' +
            '"username":"' + roomlist[roomindex].namelist[i] + '",' +
            '"botflag":"' + roomlist[roomindex].botFlaglist[i] + '",' +
            '"photo":"' + roomlist[roomindex].playerphotos[i] + '",' +
            '"points":"' + points[i] + '",' +
            '"level":"' + levels[i] + '"},';
            */
            
        mydata = mydata + '{' +
            '"userid":"' + roomlist[roomindex].playerlist[i] + '",' +
            '"username":"' + roomlist[roomindex].namelist[i] + '",' +
            '"botflag":"' + roomlist[roomindex].botFlaglist[i] + '",' +
            '"photo":"' + roomlist[roomindex].playerphotos[i] + '",' +
            '"points":"' + points[i] + '",' +
            '"level":"' + levels[i] + '",' +
            '"exp":"' + exps[i] + '",' +
            '"winning_amount":"' + winning_amounts[i] + '",' +
            '"two_played":"' + two_playeds[i].played + '",' +
            '"two_won":"' + two_playeds[i].won + '",' +
            '"four_played":"' + four_playeds[i].played + '",' +
            '"four_won":"' + four_playeds[i].won + '",' +
            '"win_rate":"' + win_streaks[i].rate + '",' +
            '"win_streak":"' + win_streaks[i].streak + '"},';
            
    }

    mydata = mydata.substring(0, mydata.length - 1);
    mydata = '{' +
        '"result":"success",' +
        '"roomid":"' + roomid + '",' +
        '"userlist": [' + mydata;
    mydata = mydata + ']}';
    console.log('---REQ_USERLIST_ROOM_RESULT---  ', JSON.parse(mydata));
    io.sockets.in('r' + roomid).emit('REQ_USERLIST_ROOM_RESULT', JSON.parse(mydata));
}
exports.AddHistory = function (data) {
    let collection = database.collection('gamehistorys');
    let currentDate = new Date();
    let currentTime =  dateFormat(currentDate, "dddd mmmm dS yyyy h:MM:ss TT");
    let query = {
        userid: data.userid,
        username : data.username,
        creater: data.creater,
        seat_limit : data.seat_limit,
        game_mode : data.gamemode,
        stake_money : parseInt(data.stake_money),
        game_status : data.game_status,
        win_money : parseInt(data.win_money),
        playing_time : currentTime,
    };
    collection.insertOne(query, function (err) {
        if (!err) {
            console.log("history info added");
        }
    });
}



function GetThisWeek() {
    let curr = new Date
    let week = []

    for (let i = 1; i <= 7; i++) {
        let first = curr.getDate() - curr.getDay() + i
        let day = new Date(curr.setDate(first)).toISOString().slice(0, 10)
        week.push(day)
        //console.log('*** ', day);
    }
    return week;
}


function msToTime(duration) {
    let milliseconds = parseInt((duration % 1000) / 100),
        seconds = Math.floor((duration / 1000) % 60),
        minutes = Math.floor((duration / (1000 * 60)) % 60),
        hours = Math.floor((duration / (1000 * 60 * 60)) % 24);

    _hours = (hours < 10) ? "0" + hours : hours;
    _minutes = (minutes < 10) ? "0" + minutes : minutes;
    _seconds = (seconds < 10) ? "0" + seconds : seconds;
    console.log("Spin Remaining: ", _hours + ":" + _minutes + ":" + _seconds + "." + milliseconds);
    let datajson = {
        result: "remaining",
        hours: hours,
        minutes: minutes,
        seconds: seconds
    }
    return datajson;
}

exports.SetTurnNextUser = function (socket, data) 
{
    console.log("SET TURN NEXT USER");
    let index = 0;
    for (index = 0; index < roomlist.length; index++) 
    {
        //console.log("Search Roomid=", data.roomid, " listRoomId=", roomlist[index].roomid);
        if (roomlist[index].roomid == data.roomid) 
        {
            let turnuser = roomlist[index].turnuser;

            for (let i = 0; i < roomlist[index].playerlist.length; i++) 
            {
                const element = roomlist[index].playerlist[i];
                if(element == turnuser)
                {
                    if(i == roomlist[index].playerlist.length - 1)
                    {
                        i = 0;
                    }
                    else
                    {
                        i++;
                    }
                    turnuser = roomlist[index].playerlist[i];
                    roomlist[index].turnuser = turnuser;
                }
            }

            io.sockets.in('r' + data.roomid).emit('REQ_TURNNEXTUSER_RESULT');
            break;
        }
    }

    setTimeout(() => {
            if (roomlist[index].playerlist.length > 0) {
                let value = randomNum(1, 6);
                roomlist[index].dice = value;
                roomlist[index].turnIndex++;
                let turndata = {
                    turnuser: roomlist[index].turnuser,
                    dice: roomlist[index].dice,
                    turnIndex: roomlist[index].turnIndex
                }
                roomlist[index].turncount = [];
                setTimeout(() => {
                    io.sockets.in('r' + data.roomid).emit('REQ_TURNUSER_RESULT', turndata);
                }, 400);
            }
        }, 100);

}

exports.SetTurnUser = function (socket, data) {
    console.log("SET TURN USER");
    let index = 0;
    for (index = 0; index < roomlist.length; index++) 
    {
        //console.log("Search Roomid=", data.roomid, " listRoomId=", roomlist[index].roomid);
        if (roomlist[index].roomid == data.roomid) 
        {
            let userid = data.userid;

            let turnuser = roomlist[index].turnuser;
            for (let i = 0; i < roomlist[index].playerlist.length; i++) 
            {
                //console.log("Search userid=", userid, " playerList=", roomlist[index].playerlist[i]);
                if (roomlist[index].playerlist[i] == userid)
                {
                    console.log("first turn user=" + userid + ", index=" + index.toString());
                    turnuser = roomlist[index].playerlist[i];
                    roomlist[index].turnuser = turnuser;
                    break;
                }
            }    
            break;
        }
    }

    setTimeout(() => {
            if (roomlist[index].playerlist.length > 0) {
                let value = randomNum(1, 6);
                roomlist[index].dice = value;
                roomlist[index].turnIndex++;
                let turndata = {
                    turnuser: roomlist[index].turnuser,
                    dice: roomlist[index].dice,
                    turnIndex: roomlist[index].turnIndex
                }
                roomlist[index].turncount = [];
                setTimeout(() => {
                    io.sockets.in('r' + data.roomid).emit('REQ_TURNUSER_RESULT', turndata);
                }, 400);
            }
        }, 100);

}

exports.GetTurnUser = function (socket, data) 
{
    console.log(getTime() + "ASK TURN USER:" + data.username);
    for (let index = 0; index < roomlist.length; index++) {
        if (roomlist[index].roomid == data.roomid) 
        {
            if (roomlist[index].turnIndex != parseInt(data.turnIndex))
            {
                console.log(getTime() + "Index Missing USER:" + data.username + ",Index:" + data.turnIndex);
                break;
            }

            let username = data.username;
            let userid = data.userid;
            //console.log(username, roomlist[index].turncount);
            let ischeck = roomlist[index].turncount.filter(function (object) {
                console.log("ASK TURN USER1");
                return (object == userid)
            });
            console.log("ischeck: ", ischeck, " playerCount:", roomlist[index].playerlist.length);
            if(ischeck.length == 0)
                roomlist[index].turncount.push(userid);

            let botCount = 0;
            for (let i = 0; i < roomlist[index].botFlaglist.length; i++)
            {
                if (roomlist[index].botFlaglist[i] == 1)
                    botCount++;
            }
            let autoCount = 0;
            for (let i = 0; i < roomlist[index].autoFlaglist.length; i++)
            {
                if (roomlist[index].autoFlaglist[i] == 1)
                    autoCount++;
            }


            console.log("bot_C: ", botCount, "auto_C:", autoCount, " turn_C:", roomlist[index].turncount.length);
            
            //if(roomlist[index].turncount.length + botCount == roomlist[index].seatlimit)
            if(roomlist[index].turncount.length + botCount + autoCount == roomlist[index].playerlist.length)
            {
                //roomlist[index].dice = parseInt(data.dice);
                SetTurn(index, data.roomid, parseInt(data.dice), data.pass); //index->room Index
                //console.log("Decide Turn");
            }
            break;
        }
    }
}

function SetTurn(index, roomid, dice, pass) 
{
    console.log("SetTurn dice= ", roomlist[index].dice);

    if (pass == "1")
        roomlist[index].dice = 1;
    else if (dice == 6)
        roomlist[index].dice = 6;

    if (roomlist[index].dice < 6)
    {
        let turnuser = roomlist[index].turnuser;
        for (let i = 0; i < roomlist[index].playerlist.length; i++) {
            const element = roomlist[index].playerlist[i];
            if(element == turnuser)
            {
                if(i == roomlist[index].playerlist.length - 1)
                {
                    i = 0;
                }
                else
                {
                    i++;
                }
                turnuser = roomlist[index].playerlist[i];
                roomlist[index].turnuser = turnuser;
            }
        }
    }
    setTimeout(() => {
        if (roomlist[index].playerlist.length > 0) {
            let value = randomNum(1, 6);
            // let value2 = randomNum(1, 3);
            // if(value == 6)
            // {
            //     if(value2 == 1)
            //     {
            //         value = randomNum(1, 5);
            //     }
            // }
            roomlist[index].dice = value;
            roomlist[index].turnIndex++;
            let turndata = {
                turnuser: roomlist[index].turnuser,
                dice: roomlist[index].dice,
                turnIndex: roomlist[index].turnIndex
            }
            roomlist[index].turncount = [];
            //io.sockets.in('r' + roomid).emit('REQ_TURNUSER_RESULT', turndata);
            setTimeout(() => {
                console.log(getTime() + "Decide Turn USER", turndata);
                io.sockets.in('r' + roomid).emit('REQ_TURNUSER_RESULT', turndata);
            }, 400);
        }
    }, 100);
}

function UpdateRoomStatus(roomid) {
    var collection = database.collection('roomdatas');
    var query = {
        roomID: roomid
    };

    collection.findOne(query, function (err, result) 
    {
        if (err) 
        {
            console.log(err);
        } 
        else 
        {
            if (result == null)
                console.log("UpdateRoomStatus failed");    
            else
            {
                console.log("roomstatus full roomid=", roomid);
                collection.updateOne(query, 
                {
                    $set: {
                        status: "full"
                    }
                }, function (err) 
                {
                    if (err) throw err;
                });                
            }
        }
    });
}

function randomNum(min, max) {
    var random = Math.floor((Math.random() * (max - min + 1)) + min);
    return random;
}

exports.ChatMessage = function (socket, data) {
    var mydata = {
        result: "success",
        username: data.username,
        message: data.message
    };
    //socket.in('r' + data.roomid).emit('REQ_CHAT_RESULT', mydata); 
    io.sockets.in('r' + data.roomid).emit('REQ_CHAT_RESULT', mydata);
};

exports.Roll_Dice = function (socket, data) {
    var roomid = data.roomid;
    for (let index = 0; index < roomlist.length; index++) {
        if (roomlist[index].roomid == roomid) {
            if (roomlist[index].dice == data.dice) {
                var mydata = {
                    roller: data.roller,
                    dice: data.dice
                };
                //console.log("REQ_ROLL_DICE_RESULT", roomid, data.roller, data.dice);
                socket.in('r' + roomid).emit('REQ_ROLL_DICE_RESULT', mydata);
                break;
            } else {
                console.log(data.roller, 'is Hacker');
            }
        }
    }
};
exports.Move_Token = function (socket, data) {
    var roomid = data.roomid;
    for (let index = 0; index < roomlist.length; index++) {
        if (roomlist[index].roomid == roomid) {
            var mydata = {
                status: data.status,
                mover: data.mover,
                movesteps: data.movesteps,
                path: data.path
            };
            roomlist[index].move_history.status = data.status;
            roomlist[index].move_history.mover = data.mover;
            roomlist[index].move_history.path = data.path;
            socket.in('r' + roomid).emit('REQ_MOVE_TOKEN_RESULT', mydata);
            console.log(roomlist[index].move_history);
            break;
        }
    }
};

exports.Move_Pawn = function (socket, data) {
    var roomid = data.roomid;
    for (let index = 0; index < roomlist.length; index++) {
        if (roomlist[index].roomid == roomid) {
            var mydata = {
                mover : data.mover,
                pawn_x : data.pawn_x,
                pawn_y : data.pawn_y,
                destination_x : data.destination_x,
                destination_y : data.destination_y,
                stopclicked : data.stopClicked,
                oppClicked : data.oppClicked,
                moveStep : data.moveStep,
            };            

            socket.in('r' + roomid).emit('REQ_MOVE_PAWN_RESULT', mydata);
            console.log(getTime() + '---REQ_MOVE_PAWN_RESULT---', mydata);
            break;
        }
    }
};

exports.Move_Stop = function (socket, data) {
    var roomid = data.roomid;
    for (let index = 0; index < roomlist.length; index++) {
        if (roomlist[index].roomid == roomid) {
            var mydata = {
                mover : data.mover,
                stop_x : data.stop_x,
                stop_y : data.stop_y,
                destination_x : data.destination_x,
                destination_y : data.destination_y,
            };            

            socket.in('r' + roomid).emit('REQ_MOVE_STOP_RESULT', mydata);
            console.log(getTime() + '---REQ_MOVE_STOP_RESULT---', mydata);
            break;
        }
    }
};



exports.Set_Auto = function (socket, data) 
{
    console.log('Set_Auto', data.username, "set auto", data.auto);
    let roomid = data.roomid;
    for (let index = 0; index < roomlist.length; index++) {
        if (roomlist[index].roomid == roomid) {
            var mydata = {
                user: data.userid,
                auto: data.auto
            };

            for (let i = 0; i < roomlist[index].namelist.length; i++)
            {
                if (roomlist[index].namelist[i] == data.username)
                {
                    roomlist[index].autoFlaglist[i] = data.auto == "true" ? 1 : 0; 
                    //roomlist[index].botFlaglist[i] = data.auto == "true" ? 1 : 0;
                    break;
                }
            }

            socket.in('r' + roomid).emit('REQ_AUTO_RESULT', mydata);
            break;
        }
    }
};
exports.LeaveRoom = function (socket, data) {
    let mydata = {
        result: "success",
        username: data.username,
        userid:data.userid,
        message: "user has left the room"
    };

    io.sockets.in('r' + data.roomid).emit('REQ_LEAVE_ROOM_RESULT', mydata);
    // socket.in('r' + data.roomid).emit('REQ_LEAVE_ROOM_RESULT', mydata);
    //socket.leave('r' + data.roomid);
    console.log(data.userid, "has ", data.roomid, "room exit");

    if (roomlist.length > 0) {
        let removeindex = null;
        for (let index = 0; index < roomlist.length; index++) {
            if (roomlist[index].roomid == data.roomid) {
                let num;
                let isExist = false;
                for (let i = 0; i < roomlist[index].playerlist.length; i++) 
                {
                    if (roomlist[index].playerlist[i] == data.userid) 
                    {
                        isExist = true;
                        num = i;

                        break;
                    }
                }
                if (isExist == true) {
                    if (roomlist[index].turnuser == data.userid) {
                        console.log('is changing turn');
                        //roomlist[index].dice = 1;
                        SetTurn(index, data.roomid, 1, "0");
                    }
                    setTimeout(() => {
                        if(roomlist[index] != undefined)
                        {
                            roomlist[index].playerlist.splice(num, 1);
                            roomlist[index].playerphotos.splice(num, 1);
                            roomlist[index].namelist.splice(num, 1);
                            roomlist[index].botFlaglist.splice(num, 1);
                            roomlist[index].autoFlaglist.splice(num, 1);
                            roomlist[index].earnScores.splice(num, 1);
                            ////exports.GetUserListInRoom(data.roomid);

                            let botCount = 0;
                            for (let i = 0; i < roomlist[index].botFlaglist.length; i++)
                            {
                                if (roomlist[index].botFlaglist[i] == 1)
                                    botCount++;
                            }

                            let AllBots = roomlist[index].playerlist.length == botCount ? true : false;

                            if (AllBots || roomlist[index].playerlist.length == 0) 
                            {
                                removeindex = index;
                                if (removeindex != null) {
                                    roomlist.splice(removeindex, 1);
                                    let query = {
                                        roomID: parseInt(data.roomid)
                                    }
                                    let collection = database.collection('roomdatas');
                                    collection.deleteOne(query, function (err, removed) {
                                        if (err) {
                                            console.log(err);
                                        } else {
                                            io.sockets.in('r' + data.roomid).emit('GAME_END', {outerid : data.userid});
                                            console.log('roomID:' + data.roomid + ' has removed successfully!');
                                        }
                                    });
                                    //roommanager.GetRoomList();
                                }
                            } 
                            else if (roomlist[index].playerlist.length == 1) 
                            {
                                //setGameResult(index, data.userid);
                                console.log("STOP! Everyone not me outsided~");
                                io.sockets.in('r' + data.roomid).emit('GAME_END', {outerid : data.userid});
                            }
                        }
                    }, 200);
                }
            }
        }

    }
}
exports.RemoveRoom = function(socket, data)
{
    console.log("Remove Force Room", data.roomid);
    let removeindex;
    for (let index = 0; index < roomlist.length; index++) {
        if (roomlist[index].roomid == data.roomid) {
            removeindex = index;
            roomlist.splice(removeindex, 1);
            let query = {
                roomID: parseInt(data.roomid)
            };
            let collection = database.collection('roomdatas');
            collection.deleteOne(query, function (err, removed) {
                if (err) {
                    console.log(err);
                } else {
                    console.log(data.roomid, 'room has removed successfully!');
                }
            });
        }
    }
}
exports.OnDisconnect = function (socket) {
    console.log("---- Disconnect -----", socket.room, socket.userid, socket.id);    
    let userdatas = database.collection('userdatas');
    userdatas.updateOne({connect:socket.id}, {
        $set: {
            status: 0,
            login_status:'0'
        }
    }, function (err) {
        if (err) throw err;
    });
    let websettings = database.collection('websettings');
    websettings.findOne({}, function(err, result)
    {
        let webdata ;
        if(err)
            console.log(err);
        if(result != null){
            if(parseInt(result.activeplayer) > 0){
                websettings.updateOne({},{$set:{activeplayer:parseInt(result.activeplayer) - 1}},function(err) {
                    if(err) throw err;                                                
                });
            }            
        }
    });

    let ischeck = socketlist.filter(function (object) {
        return (object == socket.id)
    });
    
    if (ischeck.length == 0) { 
        console.log("re-connected user");
    }
    else{
        socketlist.splice(socketlist.indexOf(socket.id),1);
        let userid = socket.userid;
        console.log("  leaving user's id : ", userid)
    
        if (socket.room == undefined || userid == undefined)
            return;
    
        let roomid_arr = socket.room.split("");
        roomid_arr.splice(0, 1);
        let roomid = '';
        for (let i = 0; i < roomid_arr.length; i++) {
            roomid += roomid_arr[i];
        }
        console.log("roomid : ", roomid);
    
        let mydata = {
        result: "success",
        userid: userid,
        message: "user has disconnected the room"
        };

        io.sockets.in('r' + roomid).emit('REQ_DISCONNECT_RESULT', mydata);

        if (roomlist.length > 0) {
            let removeindex = null;
            for (let index = 0; index < roomlist.length; index++) {
                if (roomlist[index].roomid == roomid) {
                    //console.log("yes");
                    let num;
                    let isExist = false;
                    for (let i = 0; i < roomlist[index].playerlist.length; i++) {
                        if (roomlist[index].playerlist[i] == userid) {
                            isExist = true;
                            //console.log("yes");
                            num = i
                            break;
                        }
                    }
                    if (isExist == true) {
                        setTimeout(() => {
                            //roomlist[index].playerlist.splice(num, 1);
                            //roomlist[index].botFlaglist.splice(num, 1);
                            //roomlist[index].playerphotos.splice(num, 1);
                            //roomlist[index].earnScores.splice(num, 1);

                            roomlist[index].botFlaglist[num] = 1;

                            let botCount = 0;
                            for (let i = 0; i < roomlist[index].botFlaglist.length; i++)
                            {
                                if (roomlist[index].botFlaglist[i] == 1)
                                    botCount++;
                            }

                            let AllBots = roomlist[index].playerlist.length == botCount ? true : false;
                            //exports.GetUserListInRoom(roomid);
                            if (AllBots || roomlist[index].playerlist.length == 0) {
                                //console.log("yes");
                                removeindex = index;
                                if (removeindex != null) {
                                    // console.log("yes");
                                    roomlist.splice(removeindex, 1);
                                    let query = {
                                        roomID: parseInt(roomid)
                                    };
                                    let collection = database.collection('roomdatas');
                                    collection.deleteOne(query, function (err, removed) {
                                        if (err) {
                                            console.log(err);
                                        } else {
                                            console.log(roomid, 'room has removed successfully!');
                                        }
                                    });
                                    //roommanager.GetRoomList();
                                }
                            } else if (roomlist[index].playerlist.length == 1) {
                                console.log("STOP", roomlist[index].roomid);
                                //roommanager.GetRoomList();
                                io.sockets.in('r' + roomlist[index].roomid).emit('GAME_END', {outerid:socket.userid});
                                //socket.in(socket.room).emit('GAME_END', {});
                            }
                        }, 100);
                    }
    
                }
            }
    
        }
    }
}


function setGameResult (roomIndex, outerid)
{
    for (let i = 0; i < roomlist[roomIndex].playerlist.length; i++) 
    {
        if (roomlist[roomIndex].playerlist[i] == outerid) 
        {

        }
    }
}

function getConnectedList ()
{
    let list = []
    
    for ( let client in io.sockets.connected )
    {
        list.push(client)
    }
    
    return list
}
exports.Pause_Game = function (socket, data)
{
    let roomid = data.roomid;
    let outerName = data.outerName;
    let outerPhone = data.outerPhone;
    let emitdata = {
        roomid : roomid,
        outerName : outerName,
        outerPhone : outerPhone
    }
    socket.in('r' + roomid).emit('REQ_PAUSE_RESULT', emitdata);
}
exports.Resume_Game = function (socket, data)
{
    let roomid = data.roomid;
    let emitdata = {
        roomid : roomid
    }
    socket.in('r' + roomid).emit('REQ_RESUME_RESULT', emitdata);
}

function getTime()
{
    let date_ob = new Date();

    // current date
    // adjust 0 before single digit date
    let date = ("0" + date_ob.getDate()).slice(-2);

    // current month
    let month = ("0" + (date_ob.getMonth() + 1)).slice(-2);

    // current year
    let year = date_ob.getFullYear();

    // current hours
    let hours = date_ob.getHours();

    // current minutes
    let minutes = date_ob.getMinutes();

    // current seconds
    let seconds = date_ob.getSeconds();

    // prints date in YYYY-MM-DD format
    //console.log(year + "-" + month + "-" + date);

    // prints date & time in YYYY-MM-DD HH:MM:SS format
    //console.log(year + "-" + month + "-" + date + " " + hours + ":" + minutes + ":" + seconds);

    // prints time in HH:MM format
    //console.log(hours + ":" + minutes);

    let time = "[" + hours + ":" + minutes + ":" + seconds + "] ";
    return time;
}
