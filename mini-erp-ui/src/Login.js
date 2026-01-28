import { useState } from "react";
import { api } from "./api";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [msg, setMsg] = useState("");

  const submit = async (e) => {
    e.preventDefault();
    setMsg("");
    try {
      const res = await api.post("/api/auth/login", { email, password });
      localStorage.setItem("token", res.data.token);
      setMsg("Logged in!");
    } catch (err) {
      setMsg(err.response?.data || "Error");
    }
  };

  return (
    <div style={{ maxWidth: 360, margin: "40px auto" }}>
      <h2>Login</h2>
      <form onSubmit={submit}>
        <input placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} /><br />
        <input placeholder="Password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} /><br />
        <button type="submit">Login</button>
      </form>
      <p>{msg}</p>
    </div>
  );
}
