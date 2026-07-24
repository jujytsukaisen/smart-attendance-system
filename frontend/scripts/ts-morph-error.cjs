const { Project, SyntaxKind } = require('ts-morph');
const path = require('path');

const project = new Project();
const pagesDir = path.join(__dirname, '../src/features/**/pages/*.tsx');
project.addSourceFilesAtPaths([pagesDir, '!**/*.test.tsx']);

let modifiedCount = 0;
const modifiedFiles = [];

for (const sourceFile of project.getSourceFiles()) {
  const filePath = sourceFile.getFilePath();
  
  // Find all variable declarations that call a hook starting with 'use' (excluding typical non-query hooks if needed, but let's assume use* that destructure is our target, or strictly use[A-Z] for queries)
  // Actually, standard TanStack query hooks in this project are like useDevices, useEmployees, useDashboardData. 
  // Let's find hooks that return an object being destructured, and the hook name isn't useAuthStore or form hooks.
  const queryHooks = [];
  
  const varDecls = sourceFile.getDescendantsOfKind(SyntaxKind.VariableDeclaration);
  
  for (const decl of varDecls) {
    const initializer = decl.getInitializer();
    if (!initializer || initializer.getKind() !== SyntaxKind.CallExpression) continue;
    
    const callExpr = initializer;
    const expression = callExpr.getExpression();
    if (expression.getKind() !== SyntaxKind.Identifier) continue;
    
    const hookName = expression.getText();
    // Exclude common non-query hooks
    if (hookName === 'useAuthStore' || hookName === 'useNavigate' || hookName === 'useParams' || hookName === 'useForm' || hookName === 'useState' || hookName === 'useTranslation' || hookName.endsWith('Mutation')) continue;
    
    // Also we only care if it's destructured
    const nameNode = decl.getNameNode();
    if (nameNode.getKind() === SyntaxKind.ObjectBindingPattern) {
      queryHooks.push(decl);
    }
  }

  if (queryHooks.length === 0) continue;

  // Check if it already renders an error state
  const text = sourceFile.getFullText();
  if (text.includes('if (isError)') || text.includes('failed to load') || text.includes('Failed to load') || text.includes('isError ||')) {
    continue;
  }

  // Determine aliases if multiple
  const isMultiple = queryHooks.length > 1;
  const errorVars = [];

  for (let i = 0; i < queryHooks.length; i++) {
    const decl = queryHooks[i];
    const bindingPattern = decl.getNameNode();
    
    const elements = bindingPattern.getElements();
    
    const hasIsError = elements.some(e => e.getPropertyNameNode()?.getText() === 'isError' || e.getNameNode().getText() === 'isError');
    if (!hasIsError) {
      const isErrorAlias = isMultiple ? `isError${i + 1}` : 'isError';
      const errorAlias = isMultiple ? `error${i + 1}` : 'error';
      
      bindingPattern.addElement(isMultiple ? `isError: ${isErrorAlias}` : 'isError');
      bindingPattern.addElement(isMultiple ? `error: ${errorAlias}` : 'error');
      
      errorVars.push({ isErrorAlias, errorAlias });
    }
  }

  if (errorVars.length === 0) continue; // nothing added

  // Now find the main component's first return statement
  // We'll find the default exported function or exported function named like the file
  const baseName = sourceFile.getBaseNameWithoutExtension();
  let componentFunc = sourceFile.getFunction(baseName) || 
                      sourceFile.getVariableDeclaration(baseName)?.getInitializerIfKind(SyntaxKind.ArrowFunction);
  
  if (!componentFunc) {
      // just find the first function or arrow function that has a JSX return
      const funcs = sourceFile.getDescendantsOfKind(SyntaxKind.FunctionDeclaration);
      const arrows = sourceFile.getDescendantsOfKind(SyntaxKind.ArrowFunction);
      const all = [...funcs, ...arrows];
      for (const f of all) {
          if (f.getDescendantsOfKind(SyntaxKind.JsxElement).length > 0 || f.getDescendantsOfKind(SyntaxKind.JsxFragment).length > 0) {
              componentFunc = f;
              break;
          }
      }
  }

  if (componentFunc) {
    const body = componentFunc.getBody();
    if (body && body.getKind() === SyntaxKind.Block) {
      const statements = body.getStatements();
      
      const firstReturnIndex = statements.findIndex(s => s.getKind() === SyntaxKind.ReturnStatement);
      if (firstReturnIndex !== -1) {
        
        let condition = isMultiple 
          ? errorVars.map(v => v.isErrorAlias).join(' || ') 
          : 'isError';
          
        let errorMsg = isMultiple 
          ? errorVars.map(v => `${v.errorAlias}?.message`).join(' || ') + ' || "Failed to load"'
          : 'error instanceof Error ? error.message : "Failed to load"';

        const errorBlock = `
  if (${condition}) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {${errorMsg}}
        </p>
      </div>
    );
  }
`;
        body.insertStatements(firstReturnIndex, errorBlock);
        
        sourceFile.saveSync();
        modifiedCount++;
        modifiedFiles.push(sourceFile.getBaseName());
      }
    }
  }
}

console.log(`Modified ${modifiedCount} files.`);
console.log(JSON.stringify(modifiedFiles, null, 2));
