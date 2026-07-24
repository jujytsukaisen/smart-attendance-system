const fs = require('fs');
const path = require('path');

const featuresDir = path.join(__dirname, '../src/features');
const features = fs.readdirSync(featuresDir).filter(f => fs.statSync(path.join(featuresDir, f)).isDirectory());

const apiMap = {};

features.forEach(feature => {
  const apiDir = path.join(featuresDir, feature, 'api');
  if (fs.existsSync(apiDir)) {
    const apiFiles = fs.readdirSync(apiDir).filter(f => f.endsWith('.ts'));
    apiFiles.forEach(file => {
      const content = fs.readFileSync(path.join(apiDir, file), 'utf8');
      
      const baseUrlMatch = content.match(/const (?:BASE_URL|[A-Z_]+_URL) = '([^']+)'/g);
      if (baseUrlMatch) {
        if (!apiMap[feature]) apiMap[feature] = [];
        baseUrlMatch.forEach(m => {
          const url = m.split("'")[1];
          apiMap[feature].push(url);
        });
      }
      
      // Also look for inline strings in apiClient calls
      const inlineMatches = content.match(/apiClient\.(?:get|post|put|delete|patch)[^(']*\('([^']+)'/g);
      if (inlineMatches) {
        if (!apiMap[feature]) apiMap[feature] = [];
        inlineMatches.forEach(m => {
          const url = m.match(/'([^']+)'/)[1];
          if (!apiMap[feature].includes(url) && !url.includes('${')) {
            apiMap[feature].push(url);
          }
        });
      }
    });
  }
});

console.log(JSON.stringify(apiMap, null, 2));
