import React, { useEffect, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { Gamepad2, Play, Search, BadgeInfo } from 'lucide-react';
import './styles.css';

function App() {
  const [games, setGames] = useState([]);
  const [query, setQuery] = useState('');
  const [selected, setSelected] = useState(null);

  useEffect(() => {
    fetch('/games-manifest.json?ts=' + Date.now())
      .then((res) => res.json())
      .then((data) => setGames(data.games || []))
      .catch(() => setGames([]));
  }, []);

  const filtered = useMemo(() => {
    return games.filter((game) => {
      const text = `${game.title} ${game.description} ${game.type}`.toLowerCase();
      return text.includes(query.toLowerCase());
    });
  }, [games, query]);

  return (
    <main className="app">
      <section className="hero">
        <div>
          <div className="eyebrow"><Gamepad2 size={18} /> Universal Arcade Hub</div>
          <h1>Drop games in. Play them from one arcade.</h1>
          <p>
            Supports static browser games, built Vite games, Unity WebGL builds, Godot HTML5 builds,
            and WebAssembly-ready games such as C# builds compiled for the web.
          </p>
        </div>
        <div className="hero-card">
          <strong>{games.length}</strong>
          <span>games detected</span>
        </div>
      </section>

      <section className="toolbar">
        <Search size={18} />
        <input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Search games..." />
      </section>

      {filtered.length === 0 ? (
        <section className="empty">
          <BadgeInfo size={28} />
          <h2>No games found yet</h2>
          <p>Drop a game folder into <code>public/games/</code>, then run <code>npm run dev</code>.</p>
        </section>
      ) : (
        <section className="grid">
          {filtered.map((game) => (
            <article className="game-card" key={game.id}>
              <div className="thumb">
                {game.thumbnail ? <img src={game.thumbnail} alt="" /> : <Gamepad2 size={42} />}
                <span>{game.type}</span>
              </div>
              <div className="content">
                <h2>{game.title}</h2>
                <p>{game.description}</p>
                <button onClick={() => setSelected(game)}><Play size={18} /> Play</button>
              </div>
            </article>
          ))}
        </section>
      )}

      {selected && (
        <section className="player">
          <div className="player-bar">
            <div>
              <strong>{selected.title}</strong>
              <span>{selected.type}</span>
            </div>
            <button onClick={() => setSelected(null)}>Close</button>
          </div>
          <iframe title={selected.title} src={selected.playUrl} allow="gamepad; fullscreen; autoplay" />
        </section>
      )}
    </main>
  );
}

createRoot(document.getElementById('root')).render(<App />);
