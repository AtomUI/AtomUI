import { dotnet } from './_framework/dotnet.js';
const output = document.querySelector('pre');
const log = console.log;
console.log = (...args) => { output.textContent += args.join(' ') + '\n'; log(...args); };
try {
    const runtime = await dotnet.create();
    const result = await runtime.runMain(runtime.getConfig().mainAssemblyName, []);
    output.dataset.done = result === 0 ? 'true' : 'error';
    output.dataset.exitCode = String(result);
} catch (error) {
    output.textContent += 'ERROR ' + String(error) + '\n';
    output.dataset.done = 'error';
}
