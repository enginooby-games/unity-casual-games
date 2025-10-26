const WebSocket = require('ws');

const PORT = 8080;
const wss = new WebSocket.Server({ port: PORT });

// Data structures
const rooms = new Map(); // roomId -> Set of user connections
const users = new Map(); // ws connection -> { userId, userName, roomId }

console.log(`WebSocket server running on port ${PORT}`);

wss.on('connection', (ws) => {
    console.log('New client connected');

    ws.on('message', (data) => {
        try {
            const message = JSON.parse(data);
            handleMessage(ws, message);
        } catch (error) {
            console.error('Error parsing message:', error);
        }
    });

    ws.on('close', () => {
        handleDisconnect(ws);
    });

    ws.on('error', (error) => {
        console.error('WebSocket error:', error);
    });
});

function handleMessage(ws, message) {
    switch (message.type) {
        case 'get-rooms':
            sendRoomList(ws);
            break;

        case 'join-room':
            joinRoom(ws, message);
            break;

        case 'leave-room':
            leaveRoom(ws);
            break;

        case 'video-frame':
            broadcastToRoom(ws, message, 'video-frame');
            break;

        case 'audio-chunk':
            broadcastToRoom(ws, message, 'audio-chunk');
            break;

        case 'text-message':
            broadcastToRoom(ws, message, 'text-message');
            break;

        default:
            console.log('Unknown message type:', message.type);
    }
}

function sendRoomList(ws) {
    const roomList = [];
    
    rooms.forEach((users, roomId) => {
        roomList.push({
            roomId: roomId,
            userCount: users.size
        });
    });

    send(ws, {
        type: 'room-list',
        data: roomList
    });
}

function joinRoom(ws, message) {
    const { roomId, userId, userName } = message;

    // Create room if it doesn't exist
    if (!rooms.has(roomId)) {
        rooms.set(roomId, new Set());
        console.log(`Room created: ${roomId}`);
    }

    const room = rooms.get(roomId);

    // Check room capacity (max 5 users)
    if (room.size >= 5) {
        send(ws, {
            type: 'error',
            data: 'Room is full'
        });
        return;
    }

    // Add user to room
    room.add(ws);
    users.set(ws, { userId, userName, roomId });

    console.log(`User ${userName} joined room ${roomId}`);

    // Notify user they joined successfully
    send(ws, {
        type: 'joined-room',
        roomId: roomId
    });

    // Notify other users in the room
    broadcastToRoomExcept(ws, roomId, {
        type: 'user-joined',
        userId: userId,
        data: { userId, userName }
    });

    // Send existing users to the new user
    room.forEach((client) => {
        if (client !== ws && client.readyState === WebSocket.OPEN) {
            const existingUser = users.get(client);
            if (existingUser) {
                send(ws, {
                    type: 'user-joined',
                    userId: existingUser.userId,
                    data: {
                        userId: existingUser.userId,
                        userName: existingUser.userName
                    }
                });
            }
        }
    });
}

function leaveRoom(ws) {
    const user = users.get(ws);
    if (!user) return;

    const { userId, roomId } = user;
    const room = rooms.get(roomId);

    if (room) {
        room.delete(ws);

        // Notify others in room
        broadcastToRoom(ws, {
            type: 'user-left',
            roomId: roomId,
            userId: userId
        }, 'user-left');

        // Delete room if empty
        if (room.size === 0) {
            rooms.delete(roomId);
            console.log(`Room deleted: ${roomId}`);
        }
    }

    users.delete(ws);
    console.log(`User ${userId} left room ${roomId}`);
}

function handleDisconnect(ws) {
    leaveRoom(ws);
    console.log('Client disconnected');
}

function broadcastToRoom(ws, message, type) {
    const user = users.get(ws);
    if (!user) return;

    const { roomId, userId } = user;
    const room = rooms.get(roomId);

    if (!room) return;

    const broadcastMessage = {
        type: type,
        roomId: roomId,
        userId: userId,
        data: message.data
    };

    room.forEach((client) => {
        if (client !== ws && client.readyState === WebSocket.OPEN) {
            send(client, broadcastMessage);
        }
    });
}

function broadcastToRoomExcept(ws, roomId, message) {
    const room = rooms.get(roomId);
    if (!room) return;

    room.forEach((client) => {
        if (client !== ws && client.readyState === WebSocket.OPEN) {
            send(client, message);
        }
    });
}

function send(ws, message) {
    if (ws.readyState === WebSocket.OPEN) {
        ws.send(JSON.stringify(message));
    }
}

// Cleanup on server shutdown
process.on('SIGINT', () => {
    console.log('\nClosing server...');
    wss.close(() => {
        console.log('Server closed');
        process.exit(0);
    });
});