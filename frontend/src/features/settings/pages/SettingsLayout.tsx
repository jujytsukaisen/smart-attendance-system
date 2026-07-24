import { Outlet } from 'react-router-dom';
import { SettingsSidebar } from '../components';

export function SettingsLayout() {
  return (
    <div className="space-y-6 max-w-6xl mx-auto pb-16">
      <div className="space-y-0.5">
        <h2 className="text-2xl font-bold tracking-tight">Settings</h2>
        <p className="text-muted-foreground">
          Manage your system configurations and preferences.
        </p>
      </div>
      <div className="flex flex-col space-y-8 lg:flex-row lg:space-x-12 lg:space-y-0">
        <aside className="lg:w-1/4">
          <SettingsSidebar />
        </aside>
        <div className="flex-1 w-full relative">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
