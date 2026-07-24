import os
import glob
import re

def fix_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # The user pasted this block inside `if (isLoading) {`:
    #   if (isError) {
    #     return (
    #       <div className="flex items-center justify-center py-10">
    #         <p className="text-destructive">
    #           {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
    #             ? error.message
    #             : "Failed to load"}
    #         </p>
    #       </div>
    #     );
    #   }
    
    # We want to pull it OUT of `if (isLoading)` OR just replace the error rendering logic with "Failed to load" 
    # But wait, if it's inside `isLoading`, it's syntactically broken logic. Let's fix the structure.
    
    # Pattern to find the badly nested `if (isError)` and extract it
    bad_nesting_pattern = re.compile(
        r'(if\s*\(\s*isLoading\s*\)\s*\{)\s*'
        r'(if\s*\(\s*isError\s*\)\s*\{.*?\n\s*\})', 
        re.DOTALL
    )
    
    if bad_nesting_pattern.search(content):
        # We move `if (isError)` to be before or after `if (isLoading)`
        content = bad_nesting_pattern.sub(r'\2\n\n  \1', content)
        
    # Also apply RULE 3 and RULE 5: if there is an error instanceof Error on `never`, 
    # the user said: "If a page only has isError and the hook does NOT expose error, replace error instanceof Error... with 'Failed to load'. Do NOT invent an error variable."
    # If the destructuring does not have `error`, the user says remove it. But wait, `error` IS in the destructuring in BranchDetailsPage.tsx! 
    # "const { data: branch, isLoading, isError, error } = useBranch(id!);"
    # Why did it say it's never? Because it was inside `if (isLoading)`. Once we move it out, it might NOT be never!
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    
    return True

if __name__ == "__main__":
    src_dir = r"c:\Users\MSI CYBORG 15\Downloads\smart-attendance-system\frontend\src"
    files = glob.glob(src_dir + "/**/*.tsx", recursive=True)
    count = 0
    for f in files:
        if fix_file(f):
            count += 1
    print(f"Processed {count} files")
