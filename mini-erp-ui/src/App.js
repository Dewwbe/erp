import { BrowserRouter, Routes, Route, Link } from "react-router-dom";
import Register from "./Register";
import Login from "./Login";
import Me from "./Me";

export default function App() {
  return (
    <BrowserRouter>
      <nav style={{ display: "flex", gap: 12, padding: 12 }}>
        <Link to="/register">Register</Link>
        <Link to="/login">Login</Link>
        <Link to="/me">Me</Link>
      </nav>
      <Routes>
        <Route path="/register" element={<Register />} />
        <Route path="/login" element={<Login />} />
        <Route path="/me" element={<Me />} />
      </Routes>
    </BrowserRouter>
  );
}
