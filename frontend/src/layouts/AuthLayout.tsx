import { Outlet } from 'react-router-dom';

const AuthLayout = () => {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-muted/40 p-4">
      <div className="w-full max-w-sm md:max-w-md lg:max-w-lg">
        <Outlet />
      </div>
    </div>
  );
};

export default AuthLayout;
