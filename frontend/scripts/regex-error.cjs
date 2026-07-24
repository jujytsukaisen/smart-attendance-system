const fs = require('fs');
const path = require('path');

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
  const originalContent = content;

  // Remove my previous block if it exists
  const myOldBlockRegex = /\s*if \((?:isError\d?(\s*\|\|\s*isError\d?)*)\) \{\s*return \(\s*<div className="flex items-center justify-center py-10">\s*<p className="text-destructive">\s*\{[^}]+\}\s*<\/p>\s*<\/div>\s*\);\s*\}/;
  
  if (myOldBlockRegex.test(content)) {
    content = content.replace(myOldBlockRegex, '');
  } else if (content.includes('if (isError)') || content.includes('failed to load') || content.includes('Failed to load') || content.includes('if(isError)')) {
    // If it has error handling but it's NOT my block, skip it
    continue;
  }

  const queryHookRegex = /const\s+\{([^}]+)\}\s*=\s*use([A-Z]\w+)\(([^)]*)\)/g;
  let match;
  let matches = [];
  while ((match = queryHookRegex.exec(content)) !== null) {
    const hookName = match[2];
    if (!['AuthStore', 'Navigate', 'Params', 'Form', 'State', 'Translation'].includes(hookName) && !hookName.endsWith('Mutation')) {
      matches.push(match);
    }
  }

  if (matches.length === 0) continue;

  const isMultiple = matches.length > 1;
  const errorVars = [];
  
  for (let i = matches.length - 1; i >= 0; i--) {
    const m = matches[i];
    let vars = m[1];
    
    const hasIsError = vars.includes('isError');
    if (!hasIsError) {
      const isErrorAlias = isMultiple ? `isError${i + 1}` : 'isError';
      const errorAlias = isMultiple ? `error${i + 1}` : 'error';
      
      const toAdd = isMultiple ? `isError: ${isErrorAlias}, error: ${errorAlias}` : 'isError, error';
      vars = vars.trim();
      if (vars.endsWith(',')) vars = vars + ' ' + toAdd;
      else vars = vars + ', ' + toAdd;
      
      const newStmt = `const { ${vars} } = use${m[2]}(${m[3]})`;
      content = content.substring(0, m.index) + newStmt + content.substring(m.index + m[0].length);
      
      errorVars.unshift({ isErrorAlias, errorAlias });
    } else {
      // It already has isError (from my previous run). Just determine aliases
      // Let's assume standard 'isError' and 'error' if it's the only one, else 'isError1' etc.
      // Wait, if it has 'isError: isError1', it's multiple.
      const isErrorAliasMatch = vars.match(/isError:\s*(isError\d+)/);
      const errorAliasMatch = vars.match(/error:\s*(error\d+)/);
      const isErrorAlias = isErrorAliasMatch ? isErrorAliasMatch[1] : 'isError';
      const errorAlias = errorAliasMatch ? errorAliasMatch[1] : 'error';
      errorVars.unshift({ isErrorAlias, errorAlias });
    }
  }

  if (errorVars.length === 0) continue;

  const returnRegex = /\n\s*return\s*\(/;
  const returnMatch = content.match(returnRegex);
  
  if (returnMatch) {
    let condition = isMultiple 
      ? errorVars.map(v => v.isErrorAlias).join(' || ') 
      : 'isError';
      
    let errorRef = isMultiple ? `(${errorVars.map(v => v.errorAlias).join(' || ')})` : 'error';

    const errorBlock = `
  if (${condition}) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {${errorRef} instanceof Error && !${errorRef}.message.toLowerCase().includes('internal server error') && !${errorRef}.message.includes('status code')
            ? ${errorRef}.message
            : "Failed to load"}
        </p>
      </div>
    );
  }
`;
    content = content.substring(0, returnMatch.index) + errorBlock + content.substring(returnMatch.index);
    
    if (content !== originalContent) {
      fs.writeFileSync(file, content, 'utf8');
      modifiedCount++;
      modifiedFiles.push(path.basename(file));
    }
  }
}

console.log(`Modified ${modifiedCount} files.`);
console.log(JSON.stringify(modifiedFiles, null, 2));
