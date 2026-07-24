import re

files_to_fix = [
    r"c:\Users\MSI CYBORG 15\Downloads\smart-attendance-system\frontend\src\features\attendance-policies\pages\AttendancePolicyDetailsPage.tsx",
    r"c:\Users\MSI CYBORG 15\Downloads\smart-attendance-system\frontend\src\features\attendance-policies\pages\EditAttendancePolicyPage.tsx",
    r"c:\Users\MSI CYBORG 15\Downloads\smart-attendance-system\frontend\src\features\attendance\pages\AttendanceDetailsPage.tsx",
    r"c:\Users\MSI CYBORG 15\Downloads\smart-attendance-system\frontend\src\features\auth\components\LoginForm.tsx",
    r"c:\Users\MSI CYBORG 15\Downloads\smart-attendance-system\frontend\src\features\dashboard\pages\DashboardPage.tsx",
    r"c:\Users\MSI CYBORG 15\Downloads\smart-attendance-system\frontend\src\features\leaves\pages\LeaveDetailsPage.tsx",
    r"c:\Users\MSI CYBORG 15\Downloads\smart-attendance-system\frontend\src\features\roles\pages\RoleDetailsPage.tsx",
    r"c:\Users\MSI CYBORG 15\Downloads\smart-attendance-system\frontend\src\features\users\pages\UserDetailsPage.tsx"
]

def remove_unused(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # DashboardPage has both isError and error unused
    if 'DashboardPage.tsx' in filepath:
        content = re.sub(r',\s*isError,\s*error', '', content)
    
    # LoginForm has error unused
    elif 'LoginForm.tsx' in filepath:
        content = re.sub(r',\s*error', '', content)
        content = re.sub(r'error,\s*', '', content)

    # The rest have isError unused
    else:
        content = re.sub(r',\s*isError', '', content)
        content = re.sub(r'isError,\s*', '', content)
        
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

for f in files_to_fix:
    try:
        remove_unused(f)
        print(f"Fixed {f}")
    except Exception as e:
        print(f"Error on {f}: {e}")
