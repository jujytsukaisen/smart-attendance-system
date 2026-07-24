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

const testFiles = findFiles(path.join(__dirname, '../src/features'), (name) => name.endsWith('.test.tsx'));

const modifications = {
  'UsersPage.test.tsx': {
    search: "expect(screen.getByText('admin@example.com')).toBeInTheDocument();",
    replace: "expect(screen.getByRole('cell', { name: 'admin@example.com' })).toBeInTheDocument();"
  },
  'RolesPage.test.tsx': {
    search: "expect(screen.getByText('Administrator')).toBeInTheDocument();",
    replace: "expect(screen.getByRole('cell', { name: 'Administrator' })).toBeInTheDocument();"
  },
  'NotificationsPage.test.tsx': {
    search: "expect(screen.getByText('System Update')).toBeInTheDocument();",
    replace: "expect(screen.getByRole('heading', { name: 'System Update' })).toBeInTheDocument();"
  },
  'EmployeesPage.test.tsx': {
    search: "expect(screen.getByText('John Doe')).toBeInTheDocument();",
    replace: "expect(screen.getByRole('cell', { name: 'John Doe' })).toBeInTheDocument();"
  },
  'LeavesPage.test.tsx': {
    search: "expect(screen.getByText('John Doe')).toBeInTheDocument();",
    replace: "expect(screen.getByRole('cell', { name: 'John Doe' })).toBeInTheDocument();"
  },
  'DepartmentsPage.test.tsx': {
    search: "expect(screen.getByText('HR Department')).toBeInTheDocument();",
    replace: "expect(screen.getByRole('cell', { name: 'HR Department' })).toBeInTheDocument();"
  },
  'DevicesPage.test.tsx': {
    search: "expect(screen.getByText('Main Gate Device')).toBeInTheDocument();",
    replace: "expect(screen.getByRole('cell', { name: 'Main Gate Device' })).toBeInTheDocument();"
  },
  'CompaniesPage.test.tsx': {
    search: "expect(screen.getByText('Test Company 1')).toBeInTheDocument();\n      expect(screen.getByText('Test Company 2')).toBeInTheDocument();",
    replace: "expect(screen.getByRole('cell', { name: 'Test Company 1' })).toBeInTheDocument();\n      expect(screen.getByRole('cell', { name: 'Test Company 2' })).toBeInTheDocument();"
  },
  'BranchesPage.test.tsx': {
    search: "expect(screen.getByText('Main Branch')).toBeInTheDocument();",
    replace: "expect(screen.getByRole('cell', { name: 'Main Branch' })).toBeInTheDocument();"
  },
  'AttendancePoliciesPage.test.tsx': {
    search: "expect(screen.getByText('Standard Policy')).toBeInTheDocument();",
    replace: "expect(screen.getByRole('cell', { name: 'Standard Policy' })).toBeInTheDocument();"
  },
  'AttendancePage.test.tsx': {
    search: "expect(screen.getByText('John Doe')).toBeInTheDocument();",
    replace: "expect(screen.getByRole('cell', { name: 'John Doe' })).toBeInTheDocument();"
  },
  'AuditLogsPage.test.tsx': {
    search: "expect(screen.getByText('User')).toBeInTheDocument();",
    replace: "const row = screen.getByRole('row', { name: /Created/i });\n      expect(within(row).getByText('User')).toBeInTheDocument();"
  }
};

let modifiedCount = 0;
const modifiedFileNames = [];

for (const file of testFiles) {
  const fileName = path.basename(file);
  if (modifications[fileName]) {
    let content = fs.readFileSync(file, 'utf8');
    const { search, replace } = modifications[fileName];
    if (content.includes(search)) {
      content = content.replace(search, replace);
      
      // If within is used, we need to import it
      if (replace.includes('within(') && !content.includes('within')) {
        content = content.replace("import { screen, waitFor } from '@testing-library/react';", "import { screen, waitFor, within } from '@testing-library/react';");
      }
      
      fs.writeFileSync(file, content, 'utf8');
      modifiedCount++;
      modifiedFileNames.push(fileName);
    }
  }
}

console.log(`Modified ${modifiedCount} files.`);
console.log(JSON.stringify(modifiedFileNames, null, 2));
