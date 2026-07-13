import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';

// Components
import Layout from './components/Layout';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import EmployeeList from './pages/EmployeeList';
import Requests from './pages/Requests';
import Absences from './pages/Absences';
import Travel from './pages/Travel';
import Internships from './pages/Internships';
import Calendar from './pages/Calendar';

function App() {
  return (
    <Router>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          
          <Route element={<Layout />}>
            <Route path="/" element={<Dashboard />} />
            <Route path="/employees" element={<EmployeeList />} />
            <Route path="/requests" element={<Requests />} />
            <Route path="/absences" element={<Absences />} />
            <Route path="/travel" element={<Travel />} />
            <Route path="/internships" element={<Internships />} />
            <Route path="/calendar" element={<Calendar />} />
          </Route>
        </Routes>
      </AuthProvider>
    </Router>
  );
}

export default App;
