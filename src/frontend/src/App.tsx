import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { Toaster } from "@fluentui/react-components";
import { ThemeProvider } from "./context/ThemeContext";
import Index from "./pages/Index.tsx";
import Login from "./pages/Login.tsx";
import Engines from "./pages/Engines.tsx";
import History from "./pages/History.tsx";
import Leaderboard from "./pages/Leaderboard.tsx";
import NotFound from "./pages/NotFound.tsx";

const queryClient = new QueryClient({
  defaultOptions: {
    // A focus-triggered refetch would race the engine-move polling loop mid-game.
    queries: { retry: 1, refetchOnWindowFocus: false },
  },
});

const App = () => (
  <ThemeProvider>
    <QueryClientProvider client={queryClient}>
      <Toaster toasterId="app-toaster" />
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Index />} />
          <Route path="/login" element={<Login />} />
          <Route path="/engines" element={<Engines />} />
          <Route path="/history" element={<History />} />
          <Route path="/leaderboard" element={<Leaderboard />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  </ThemeProvider>
);

export default App;
