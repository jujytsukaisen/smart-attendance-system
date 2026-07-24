const fs = require('fs');
const path = require('path');

const featuresDir = path.join(__dirname, 'src', 'features');
const features = fs.readdirSync(featuresDir).filter(f => fs.statSync(path.join(featuresDir, f)).isDirectory());

const results = {};

function findFiles(dir, filter, fileList = []) {
  if (!fs.existsSync(dir)) return fileList;
  const files = fs.readdirSync(dir);
  for (const file of files) {
    const stat = fs.statSync(path.join(dir, file));
    if (stat.isDirectory()) {
      findFiles(path.join(dir, file), filter, fileList);
    } else if (filter(file)) {
      fileList.push(path.join(dir, file));
    }
  }
  return fileList;
}

features.forEach(f => {
  const apiDir = path.join(featuresDir, f, 'api');
  let apiEndpoint = null;
  if (fs.existsSync(apiDir)) {
    const tsFiles = findFiles(apiDir, name => name.endsWith('.ts') && !name.endsWith('.test.ts'));
    tsFiles.forEach(file => {
      const content = fs.readFileSync(file, 'utf8');
      const match = content.match(/const (?:BASE_URL|ROLES_URL)\s*=\s*'([^']+)'/);
      if (match) apiEndpoint = match[1];
      else {
        const directMatch = content.match(/apiClient\.(?:get|post|put|delete|patch)(?:<[^>]+>)?\('([^']+)'/);
        if (directMatch) apiEndpoint = directMatch[1];
      }
    });
  }

  const testFiles = findFiles(path.join(featuresDir, f), name => name.endsWith('.test.tsx') || name.endsWith('.test.ts'));
  const mocks = [];
  testFiles.forEach(testFile => {
    const content = fs.readFileSync(testFile, 'utf8');
    const regex = /http\.(get|post|put|delete|patch)\(`\${BASE_URL}([^`]+)`/g;
    let match;
    while ((match = regex.exec(content)) !== null) {
      mocks.push({ file: path.basename(testFile), method: match[1], url: match[2] });
    }
  });

  results[f] = { apiEndpoint, mocks };
});

console.log(JSON.stringify(results, null, 2));
