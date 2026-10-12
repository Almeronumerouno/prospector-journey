/**
 * Unity MCP Client Bridge Runner
 * Connects to Unity Official MCP relay (relay_win.exe --mcp)
 * Usage: node scripts/unity_mcp.js <toolName> [jsonArgs]
 */
const { spawn } = require('child_process');
const path = require('path');
const os = require('os');

const RELAY_PATH = path.join(os.homedir(), '.unity', 'relay', 'relay_win.exe');

async function callUnityTool(toolName, args = {}) {
  return new Promise((resolve, reject) => {
    const proc = spawn(RELAY_PATH, ['--mcp']);
    let buffer = '';
    let settled = false;

    const timeout = setTimeout(() => {
      if (!settled) {
        settled = true;
        proc.kill();
        reject(new Error(`Timeout calling Unity MCP tool: ${toolName}`));
      }
    }, 15000);

    proc.stdout.on('data', (data) => {
      buffer += data.toString();
      const lines = buffer.split('\n');
      buffer = lines.pop();

      for (const line of lines) {
        if (!line.trim()) continue;
        try {
          const msg = JSON.parse(line);
          if (msg.id === 1) {
            // Handshake completed, send initialized notification and call tool
            proc.stdin.write(JSON.stringify({ jsonrpc: '2.0', method: 'notifications/initialized' }) + '\n');
            proc.stdin.write(JSON.stringify({
              jsonrpc: '2.0',
              id: 2,
              method: 'tools/call',
              params: {
                name: toolName,
                arguments: args
              }
            }) + '\n');
          } else if (msg.id === 2) {
            settled = true;
            clearTimeout(timeout);
            proc.kill();
            if (msg.error) {
              reject(new Error(msg.error.message || JSON.stringify(msg.error)));
            } else {
              resolve(msg.result);
            }
          }
        } catch (e) {
          // ignore non-json log lines
        }
      }
    });

    proc.stderr.on('data', () => {});

    proc.on('error', (err) => {
      if (!settled) {
        settled = true;
        clearTimeout(timeout);
        reject(err);
      }
    });

    const initReq = JSON.stringify({
      jsonrpc: '2.0',
      id: 1,
      method: 'initialize',
      params: {
        protocolVersion: '2024-11-05',
        capabilities: {},
        clientInfo: { name: 'antigravity-jarvis', version: '1.0' }
      }
    }) + '\n';

    proc.stdin.write(initReq);
  });
}

// CLI execution
if (require.main === module) {
  const toolName = process.argv[2] || 'Unity_GetConsoleLogs';
  let toolArgs = {};
  if (process.argv[3]) {
    try {
      toolArgs = JSON.parse(process.argv[3]);
    } catch {
      toolArgs = {};
    }
  }

  callUnityTool(toolName, toolArgs)
    .then((res) => {
      if (res && res.content && res.content[0] && res.content[0].text) {
        try {
          const parsed = JSON.parse(res.content[0].text);
          console.log(JSON.stringify(parsed, null, 2));
        } catch {
          console.log(res.content[0].text);
        }
      } else {
        console.log(JSON.stringify(res, null, 2));
      }
      process.exit(0);
    })
    .catch((err) => {
      console.error('Error:', err.message);
      process.exit(1);
    });
}

module.exports = { callUnityTool };
