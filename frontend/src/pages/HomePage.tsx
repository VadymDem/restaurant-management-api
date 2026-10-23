import { Link } from 'react-router-dom'
import './HomePage.css'

const highlights = [
  {
    number: '01',
    title: 'A menu worth exploring',
    description:
      'Discover a thoughtful selection of dishes prepared with quality ingredients.',
  },
  {
    number: '02',
    title: 'Your table, your time',
    description:
      'Plan your visit with a straightforward reservation experience built around you.',
  },
  {
    number: '03',
    title: 'Made for good moments',
    description:
      'From a quiet dinner to a special celebration, every visit should feel memorable.',
  },
]

function HomePage() {
  return (
    <div className="home-page">
      <section className="home-hero" aria-labelledby="home-heading">
        <div className="home-hero__glow" aria-hidden="true" />

        <div className="home-hero__content">
          <p className="home-eyebrow">Dining, beautifully arranged</p>
          <h1 id="home-heading">
            Your table is
            <span> waiting.</span>
          </h1>
          <p className="home-hero__description">
            Great evenings begin with a place at the table. Explore our menu and
            make your next restaurant visit one to remember.
          </p>

          <div className="home-hero__actions">
            <Link className="home-button home-button--primary" to="/menu">
              Explore the menu
              <span aria-hidden="true">→</span>
            </Link>
            <Link className="home-button home-button--ghost" to="/register">
              Create an account
            </Link>
          </div>
        </div>

        <div className="home-hero__details" aria-label="Restaurant highlights">
          <div>
            <span>01</span>
            <p>Curated dishes</p>
          </div>
          <div>
            <span>02</span>
            <p>Easy reservations</p>
          </div>
          <div>
            <span>03</span>
            <p>Warm hospitality</p>
          </div>
        </div>
      </section>

      <section className="home-intro">
        <div className="home-intro__heading">
          <p className="home-eyebrow">The restaurant experience</p>
          <h2>More than a reservation</h2>
        </div>

        <div className="home-intro__copy">
          <p>
            We bring the essential parts of your visit together in one calm,
            simple experience — from discovering your next favourite dish to
            planning the perfect evening.
          </p>
          <Link to="/menu">
            See what is on the menu <span aria-hidden="true">↗</span>
          </Link>
        </div>
      </section>

      <section className="home-highlights" aria-label="Why dine with us">
        {highlights.map((highlight) => (
          <article className="home-highlight" key={highlight.number}>
            <span className="home-highlight__number">{highlight.number}</span>
            <div>
              <h3>{highlight.title}</h3>
              <p>{highlight.description}</p>
            </div>
          </article>
        ))}
      </section>

      <section className="home-feature">
        <div className="home-feature__image" role="img" aria-label="Elegant restaurant interior">
          <div className="home-feature__badge">
            <strong>Made for</strong>
            <span>unforgettable evenings</span>
          </div>
        </div>

        <div className="home-feature__content">
          <p className="home-eyebrow">A seat at our table</p>
          <h2>Come for the food. Stay for the feeling.</h2>
          <p>
            Thoughtful details, welcoming spaces and a menu created to be shared.
            Everything is ready — all that is missing is you.
          </p>

          <div className="home-feature__divider" />

          <blockquote>
            “The best memories are made around the table.”
          </blockquote>
        </div>
      </section>

      <section className="home-cta">
        <p className="home-eyebrow">Your next evening starts here</p>
        <h2>Ready to take a seat?</h2>
        <p>Browse the menu, create your account and let us take care of the rest.</p>
        <div className="home-cta__actions">
          <Link className="home-button home-button--light" to="/menu">
            View our menu
          </Link>
          <Link className="home-button home-button--outline" to="/register">
            Get started
          </Link>
        </div>
      </section>
    </div>
  )
}

export default HomePage
