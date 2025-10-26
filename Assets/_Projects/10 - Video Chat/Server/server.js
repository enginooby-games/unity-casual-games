// --- Imports ---
const http = require('http');
const express = require('express');
const WebSocket = require('ws');

// --- Express Setup (so Replit/Render gives us an HTTPS endpoint) ---
const app = express();
const server = http.createServer(app);
const wss = new WebSocket.Server({ server });

// --- Port setup ---
const PORT = process.env.PORT || 8080;

// --- Data Structures ---
const rooms = new Map(); // roomId -> Set of user connections
const users = new Map(); // ws connection -> { userId, userName, roomId }

console.log(`WebSocket server running on port ${PORT}`);

// --- WebSocket Handling ---
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

    ws.on('close', () => handleDisconnect(ws));
    ws.on('error', (error) => console.error('WebSocket error:', error));
});

// --- Express Route (optional test) ---
app.get('/', (req, res) => {
    res.send('✅ WebSocket Signaling Server is running.');
});

// --- Message Handlers ---

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

    rooms.forEach((roomData, roomId) => {
        roomList.push({
            roomId: roomId,
            userCount: roomData.users.size,
            hasPassword: roomData.password !== ""
        });
    });

    send(ws, {
        type: 'room-list',
        data: roomList
    });
}

function joinRoom(ws, message) {
    const { roomId, userId, userName, password = "" } = message;

    // Create room if it doesn't exist
    if (!rooms.has(roomId)) {
        rooms.set(roomId, {
            users: new Set(),
            password: password // First user sets the password
        });
        console.log(`Room created: ${roomId}${password ? ' (password protected)' : ''}`);
    }

    const roomData = rooms.get(roomId);

    // Check password
    if (roomData.password !== "" && roomData.password !== password) {
        send(ws, {
            type: 'join-error',
            data: 'Incorrect password'
        });
        console.log(`User ${userName} failed to join room ${roomId}: wrong password`);
        return;
    }

    // Check room capacity (max 5 users)
    if (roomData.users.size >= 5) {
        send(ws, {
            type: 'join-error',
            data: 'Room is full'
        });
        console.log(`User ${userName} failed to join room ${roomId}: room full`);
        return;
    }

    // Add user to room
    roomData.users.add(ws);
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
    roomData.users.forEach((client) => {
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
    const roomData = rooms.get(roomId);

    if (roomData) {
        roomData.users.delete(ws);

        // Notify others in room
        broadcastToRoom(ws, {
            type: 'user-left',
            roomId: roomId,
            userId: userId
        }, 'user-left');

        // Delete room if empty
        if (roomData.users.size === 0) {
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
    const roomData = rooms.get(roomId);

    if (!roomData) return;

    const broadcastMessage = {
        type: type,
        roomId: roomId,
        userId: userId,
        data: message.data
    };

    roomData.users.forEach((client) => {
        if (client !== ws && client.readyState === WebSocket.OPEN) {
            send(client, broadcastMessage);
        }
    });
}

function broadcastToRoomExcept(ws, roomId, message) {
    const roomData = rooms.get(roomId);
    if (!roomData) return;

    roomData.users.forEach((client) => {
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

// --- Start Server ---
server.listen(PORT, () => console.log(`✅ Server running on port ${PORT}`));
