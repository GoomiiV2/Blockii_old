"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const http = require("http");
var port = process.env.port || 8080;
http.createServer(function (req, res) {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Hello World\n');
}).listen(port);
//# sourceMappingURL=server.js.map