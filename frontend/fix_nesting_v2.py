import os
import glob
import re

def fix_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    original_content = content
    
    # 1. Fix nested `if (isError)` inside `if (isLoadingX)`
    bad_nesting_pattern = re.compile(
        r'(if\s*\(\s*isLoading[A-Za-z0-9_]*\s*\)\s*\{)\s*'
        r'(if\s*\(\s*isError\s*\)\s*\{.*?\n\s*\})', 
        re.DOTALL
    )
    
    if bad_nesting_pattern.search(content):
        content = bad_nesting_pattern.sub(r'\2\n\n  \1', content)

    # 2. Fix the duplicated `if (isError || !entity)` block that causes `error` to be `never`
    # Pattern: `if (isError || !branch)` -> `if (!branch)`
    # And `<p>{error?.message || 'Branch not found'}</p>` -> `<p>Branch not found</p>`
    # OR if it's `error?.message || '...'`
    
    # We will just look for `if (isError || !` and replace with `if (!`
    content = re.sub(r'if\s*\(\s*isError\s*\|\|\s*!', r'if (!', content)
    
    # Then we replace `{error\?\.message\s*\|\|\s*([^}]+)\}` with `{\1}`
    content = re.sub(r'\{error\?\.message\s*\|\|\s*([^}]+)\}', r'{\1}', content)

    # 3. For TS2531 Object is possibly 'null' in AttendanceReportsPage and DeviceDetailsPage:
    # "error1 || error2" -> "currentError" as per RULE 4.
    # The rule says: Replace code like `(error1 || error2).message` with `const currentError = error1 ?? error2;` and then `if (currentError instanceof Error) { ... }`
    
    if content != original_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

if __name__ == "__main__":
    src_dir = r"c:\Users\MSI CYBORG 15\Downloads\smart-attendance-system\frontend\src"
    files = glob.glob(src_dir + "/**/*.tsx", recursive=True)
    count = 0
    for f in files:
        if fix_file(f):
            count += 1
            print("Fixed:", f)
    print(f"Processed and fixed {count} files")
