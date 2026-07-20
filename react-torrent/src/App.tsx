import { useEffect, useState } from 'react';
import {
  HashRouter as Router,
  Redirect,
  Route
} from "react-router-dom";
import ProtectedRoute from './components/ProtectedRoute';
import { GlobalContext } from './GlobalContext';
import QbittorrentService from './services/qbittorrent';
import Dashboard from './views/Dashboard';
import Login from './views/Login';

function App() {
  // Global context state for the app
  const [authenticated, setAuthenticated] = useState<boolean | null>(null);
  const [qbittorrentService] = useState(new QbittorrentService(setAuthenticated))

  const globalContext = {
    authenticated,
    setAuthenticated,
    qbittorrentService
  }

  useEffect(() => {
    qbittorrentService.validateAuthentication();
  }, [qbittorrentService]);

  return (
    <div>
      <GlobalContext.Provider value={{ ...globalContext }} >
        {authenticated !== null &&
          <Router>
            {/* Redirect default path to the dashboard */}
            <Route path="/">
              <Redirect to={{ pathname: "/dashboard" }} />
            </Route>
            {/* Login route */}
            <Route path="/login">
              <Login />
            </Route>
            {/* Dashboard route */}
            <ProtectedRoute path="/dashboard" authenticated={!!authenticated} component={Dashboard} />
          </Router>}
      </GlobalContext.Provider>
    </div>
  );
}

export default App;
