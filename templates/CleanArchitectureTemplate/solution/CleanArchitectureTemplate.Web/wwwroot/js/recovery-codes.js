// Downloads the recovery codes shown on Manage/DisplayRecoveryCodes as a
// plain-text file. Expects #saveCodesBtn and .recovery-code elements.
const saveButton = document.getElementById('saveCodesBtn');

saveButton?.addEventListener('click', () => {
    const codes = [...document.querySelectorAll('#recoveryCodes .recovery-code')]
        .map((element) => element.textContent.trim())
        .filter((code) => code.length > 0);

    const content = [
        'Recovery codes',
        `Generated: ${new Date().toISOString()}`,
        '',
        ...codes,
        '',
        'Each code can be used only once. Store this file in a safe place.',
    ].join('\n');

    const url = URL.createObjectURL(new Blob([content], { type: 'text/plain' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = 'recovery-codes.txt';
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(url);
});