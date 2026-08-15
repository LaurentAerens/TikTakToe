import { useState } from "react";
import type { ReactNode } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
  NavDrawer,
  NavDrawerBody,
  NavDrawerHeader,
  NavDrawerFooter,
  NavItem,
  NavSectionHeader,
  Text,
  tokens,
  Button,
} from "@fluentui/react-components";
import {
  Grid24Regular,
  BotRegular,
  History24Regular,
  TrophyRegular,
  Settings24Regular,
  SignOut24Regular,
  Navigation24Regular,
  Dismiss24Regular,
} from "@fluentui/react-icons";

interface AppLayoutProps {
  children: ReactNode;
}

const mainItems = [
  { title: "Play", url: "/", icon: <Grid24Regular />, value: "play" },
  { title: "Engines", url: "/engines", icon: <BotRegular />, value: "engines" },
  { title: "History", url: "/history", icon: <History24Regular />, value: "history" },
  { title: "Leaderboard", url: "/leaderboard", icon: <TrophyRegular />, value: "leaderboard" },
];

const bottomItems = [
  { title: "Settings", url: "/settings", icon: <Settings24Regular />, value: "settings" },
  { title: "Login", url: "/login", icon: <SignOut24Regular />, value: "login" },
];

const AppLayout = ({ children }: AppLayoutProps) => {
  const location = useLocation();
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);

  const allItems = [...mainItems, ...bottomItems];
  const selected = allItems.find((i) => i.url === location.pathname)?.value ?? "";

  const isMobile = typeof window !== "undefined" && window.innerWidth < 768;

  return (
    <div
      className="min-h-screen flex w-full"
      style={{
        backgroundColor: tokens.colorNeutralBackground1,
        color: tokens.colorNeutralForeground1,
      }}
    >
      {/* Desktop: inline drawer (always visible, full height, no fly-in) */}
      <NavDrawer
        open
        type="inline"
        selectedValue={selected}
        size="small"
        className="hidden md:flex app-desktop-nav"
        style={{
          borderRight: `1px solid ${tokens.colorNeutralStroke2}`,
        }}
      >
        <NavDrawerHeader>
          <div className="flex items-center gap-2 px-2 py-1">
            <div
              className="w-8 h-8 rounded-md flex items-center justify-center"
              style={{
                backgroundColor: tokens.colorBrandBackground2,
                color: tokens.colorBrandForeground1,
              }}
            >
              <Grid24Regular />
            </div>
            <div>
              <Text weight="semibold" size={300}>TTT Engine</Text>
              <div>
                <Text size={100} style={{ color: tokens.colorNeutralForeground3 }}>v1.0</Text>
              </div>
            </div>
          </div>
        </NavDrawerHeader>

        <NavDrawerBody>
          <NavSectionHeader>Game</NavSectionHeader>
          {mainItems.map((item) => (
            <NavItem
              key={item.value}
              icon={item.icon}
              value={item.value}
              onClick={() => navigate(item.url)}
            >
              {item.title}
            </NavItem>
          ))}
        </NavDrawerBody>

        <NavDrawerFooter>
          <div className="flex flex-col gap-1 w-full">
            <NavSectionHeader>Account</NavSectionHeader>
            {bottomItems.map((item) => (
              <NavItem
                key={item.value}
                icon={item.icon}
                value={item.value}
                onClick={() => navigate(item.url)}
              >
                {item.title}
              </NavItem>
            ))}
          </div>
        </NavDrawerFooter>
      </NavDrawer>

      {/* Mobile: overlay drawer with hamburger */}
      <div className="md:hidden">
        <Button
          appearance="transparent"
          icon={<Navigation24Regular />}
          onClick={() => setMobileOpen(true)}
          style={{
            position: "fixed",
            top: 12,
            left: 12,
            zIndex: 1000,
          }}
          aria-label="Open menu"
        />
        <NavDrawer
          open={mobileOpen}
          type="overlay"
          onOpenChange={(_, { open }) => setMobileOpen(open)}
          selectedValue={selected}
          size="small"
        >
          <NavDrawerHeader>
            <div className="flex items-center justify-between px-2 py-1">
              <div className="flex items-center gap-2">
                <div
                  className="w-8 h-8 rounded-md flex items-center justify-center"
                  style={{
                    backgroundColor: tokens.colorBrandBackground2,
                    color: tokens.colorBrandForeground1,
                  }}
                >
                  <Grid24Regular />
                </div>
                <div>
                  <Text weight="semibold" size={300}>TTT Engine</Text>
                  <div>
                    <Text size={100} style={{ color: tokens.colorNeutralForeground3 }}>v1.0</Text>
                  </div>
                </div>
              </div>
              <Button
                appearance="transparent"
                icon={<Dismiss24Regular />}
                onClick={() => setMobileOpen(false)}
                aria-label="Close menu"
              />
            </div>
          </NavDrawerHeader>

          <NavDrawerBody>
            <NavSectionHeader>Game</NavSectionHeader>
            {mainItems.map((item) => (
              <NavItem
                key={item.value}
                icon={item.icon}
                value={item.value}
                onClick={() => {
                  navigate(item.url);
                  setMobileOpen(false);
                }}
              >
                {item.title}
              </NavItem>
            ))}
          </NavDrawerBody>

          <NavDrawerFooter>
            <div className="flex flex-col gap-1 w-full">
              <NavSectionHeader>Account</NavSectionHeader>
              {bottomItems.map((item) => (
                <NavItem
                  key={item.value}
                  icon={item.icon}
                  value={item.value}
                  onClick={() => {
                    navigate(item.url);
                    setMobileOpen(false);
                  }}
                >
                  {item.title}
                </NavItem>
              ))}
            </div>
          </NavDrawerFooter>
        </NavDrawer>
      </div>

      <div className="flex-1 flex flex-col min-w-0">
        <main className="flex-1 p-4 md:p-6" style={{ paddingTop: isMobile ? 56 : undefined }}>
          {children}
        </main>
      </div>
    </div>
  );
};

export default AppLayout;
