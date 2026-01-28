import { useState } from "react";
import { api } from "./api";

export default function Me() {
  const [me, setMe] = useState(null);
  const [msg, setMsg] = useState("");

  const load = async () => {
    setMsg("");
    try {
      const res = await api.get("/api/users/me");
      setMe(res.data);
    } catch {
      setMsg("Not authorized. Login first.");
    }
  };

  return (
    <div style={{ maxWidth: 360, margin: "40px auto" }}>
      <h2>My Profile</h2>
      <button onClick={load}>Load /me</button>
      {msg && <p>{msg}</p>}
      <pre>{me ? JSON.stringify(me, null, 2) : ""}</pre>
    </div>
  );
}
