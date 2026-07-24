const fs = require('fs');
const path = require('path');
const glob = require('glob'); // Not installed? I'll use simple fs traversal.

function findFiles(dir, filter, fileList = []) {
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

const pagesDir = path.join(__dirname, '../src/features');
const pageFiles = findFiles(pagesDir, (name) => name.endsWith('Page.tsx') && !name.includes('.test.tsx'));

let modifiedCount = 0;
const modifiedFiles = [];

for (const file of pageFiles) {
  let content = fs.readFileSync(file, 'utf8');
  let originalContent = content;

  // Check if it already has isError handling for the main query
  if (content.includes('if (isError)')) {
    continue;
  }

  // Find query hook destructuring: const { data, isLoading } = useSomething(...)
  const queryHookRegex = /const\s+\{\s*([^}]+)\s*\}\s*=\s*use([A-Z]\w+)\(([^)]*)\)/;
  const match = content.match(queryHookRegex);

  if (match) {
    let vars = match[1];
    
    // Check if isError is already extracted
    if (!vars.includes('isError')) {
      vars = vars.trim() + ', isError, error';
      content = content.replace(match[0], `const { ${vars} } = use${match[2]}(${match[3]})`);
    }

    // Now insert the error block before the FIRST top-level return (
    const returnRegex = /\s+return\s*\(/;
    const returnMatch = content.match(returnRegex);
    
    if (returnMatch) {
      const errorBlock = `
  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error ? error.message : "Failed to load"}
        </p>
      </div>
    );
  }
`;
      // Replace the first 'return (' with the error block followed by 'return ('
      content = content.substring(0, returnMatch.index) + errorBlock + content.substring(returnMatch.index);
      
      if (content !== originalContent) {
        fs.writeFileSync(file, content, 'utf8');
        modifiedCount++;
        modifiedFiles.push(file);
      }
    }
  }
}

console.log(`Modified ${modifiedCount} files.`);
console.log(JSON.stringify(modifiedFiles, null, 2));
