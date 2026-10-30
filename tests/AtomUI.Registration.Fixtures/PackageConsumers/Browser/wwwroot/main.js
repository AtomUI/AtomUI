import { dotnet } from './_framework/dotnet.js';
const output = document.querySelector('pre');
const log = console.log;
console.log = (...args) => {
    const message = args.join(' ');
    output.textContent += message + '\n'; log(...args);
    if (message === 'COLD_BROWSER_PASS ordinary binary adapter template' && new URLSearchParams(location.search).get('report') === '1') {
        fetch('/__fixture_result', { method: 'POST', body: message }).catch(() => {});
    }
};
try {
    const runtime = await dotnet.create();
    const result = await runtime.runMain(runtime.getConfig().mainAssemblyName, []);
    output.dataset.done = result === 0 ? 'true' : 'error';
    output.dataset.exitCode = String(result);
} catch (error) {
    output.textContent += 'ERROR ' + String(error) + '\n';
    output.dataset.done = 'error';
}
