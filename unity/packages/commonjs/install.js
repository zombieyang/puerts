const cp = require('child_process');
const path = require('path');
const fs = require('fs');

const cwd = process.cwd();
let parentPackageJSONLocation = path.resolve(cwd, "..");
while (!fs.existsSync(path.join(parentPackageJSONLocation, 'package.json')))
{
    const old = parentPackageJSONLocation;
    parentPackageJSONLocation = path.resolve(parentPackageJSONLocation, "..");
    if (parentPackageJSONLocation == old) {
        throw new Error("cannot find puer-project's package.json");
    }
}
cp.execSync(`openupm add "${process.argv[2]}@file:${path.join(cwd, process.argv[3])}"`, {
    cwd: path.resolve(parentPackageJSONLocation, '..')
})