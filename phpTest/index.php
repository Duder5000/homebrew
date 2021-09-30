<!DOCTYPE html>
<html>
  <head>
    <meta charset="UTF-8">
    <title>PHP Test</title>
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/normalize/8.0.1/normalize.min.css" integrity="sha512-NhSC1YmyruXifcj/KFRWoC561YpHpc5Jtzgvbuzx5VozKpWvQ+4nXhPdFgmx8xqexRcpAglTj9sIBWINXa8x5w==" crossorigin="anonymous">
    <link rel="stylesheet" href="css/w3.css">
    <link rel="stylesheet" href="css/darkmode.css">
    <link rel="icon" href="img/duder-logo.png">
  </head>

  <body>
    <div class="w3-content" style="max-width:1400px">

    <header class="w3-container w3-center"> 
      <h1><a href="index.html" class="home-link">PHP Test</a></h1>
    </header>

    <div class="w3-row">

    <div class="w3-col l8 s12">
      <div class="w3-card-4 w3-margin card">
        <div id="published" class="w3-container">
          <h2>
            <b>Published Subclasses</b>
          </h2>

          <h3>Fighter</h3>
          <ul>
            <li>
              <a href="sofw-arcane-archer-redux.html">Arcane Archer Redux</a> <p class="book sofw">(SoFW)</p>
              <p class="short-desc">
               A revised version of the Arcane Archer Martial Archetype.</br>
               An Arcane Archer studies a unique elven method of archery that weaves magic into attacks to produce supernatural effects.
              </p>
            </li>
          </ul>

          <h3>Ranger</h3>
            <ul>
              <li><a href="ranger-spells.html">Ranger Revised (Known Spells)</a> <p class="book sofw">(SoFW)</p></li>
              <p class="short-desc">
                Additional spell for the Beast Master and Hunter Ranger Subclasses when you reach certain levels in the class.
              </p>
            </ul>

          <h3>Rogue</h3>
          <ul>
            <li>
              <a href="sofw-master-thrower.html">Master Thrower</a> <p class="book sofw">(SoFW)</p>
              <p class="short-desc">
                You have honed your skills to become a master all thrown weapons. You spent your years before adventuring learning pub tricks to con other patrons out of gold, were raised by the circus, or any other experiences that honed your skills. 
              </p>
            </li>
          </ul>

          <h3>Sorcerer</h3>
          <ul>
            <li>
              <a href="eots-plagueblessed.html">Plagueblessed</a> <p class="book eots">(EotS)</p>
              <p class="short-desc">
                You were born of two that bare the mark of the Spellplague, the Spellplague is part of your blood. You were trained from birth under the watchful eye of the High Council to be their enforcer. 
              </p>
            </li>
            <li>
              <a href="aofl-gem-draconic.html">Gem Draconic Bloodline</a> <p class="book odvaskar">(AoFL+)</p>
              <p class="short-desc">
                An addition to the Draconic Bloodline Sorcerer, adding Gem Dragon options. 
              </p>
            </li>
          </ul>

          <h3>Warlock</h3>
          <ul>
            <li>
              <a href="eots-holy-flame.html">Holy Flame</a> <p class="book eots">(EotS)</p>
              <p class="short-desc">
                You were selected by The High Council of the Holy Flame to archive; the history of the Holy Flame. When required you can use your knowledge to aid your allies or hinder your enemies.
              </p>
            </li>
            <li>
              <a href="sofw-slaad-lord.html">Slaad Lord</a> <p class="book sofw">(SoFW)</p>
              <p class="short-desc">
                You have made a pact with a Slaad Lord from the plane of Limbo, a being of pure chaos. They are the closest thing that the Slaadi have to actual deities, but they do not command any worship. 
              </p>
            </li>
          </ul>

        </div>
      </div>

      <div class="w3-card-4 w3-margin card">
        <div id="spells" class="w3-container">
          <h2><b>Spell Book</b> <a href="spells.html"><img class="link-icon" src="img/icons/spellbook.png"></a></h2>


          <h3 class="inline-h">            
            <a href="spells.html#blight-touch">Blight Touch</a>
            <img class="spell-icon" src="img/icons/necromancy.png"> <p class="spell-level">-0-</p>
          </h3>
          <p class="book sofw">(SoFW)</p>

          <ul>
            <li>
              <p class="short-desc">
                You touch a creature causing decay to spread through their body. 
                Make a melee spell attack against one creature within 5ft of you. 
                On a hit, the target takes 1d4 necrotic damage and must make a Constitution saving throw. 
                On a failed save, it is also <a href="https://www.dndbeyond.com/sources/basic-rules/appendix-a-conditions#Poisoned" target="_blank">poisoned</a> until the start of your next turn. 
              </p>
            </li>
          </ul>

          <h3 class="inline-h">            
            <a href="spells.html#spellscar-blade">Spellscar Blade</a>
            <img class="spell-icon" src="img/icons/evocation.png"> <p class="spell-level">1st</p>
          </h3>
          <p class="book eots">(EotS)</p>

          <ul>
            <li>
              <p class="short-desc">
                You draw forth the energy of your Spellscar to form a sword of blue fire in your hand. This magic sword lasts until the spell ends. It counts as a simple melee weapon with which you are proficient. It deals 1d8 fire damage on a hit and has the <em>finesse</em> and <em>light</em> properties. 
                Additionally, when you attack a creature that has already taken fire damage this round, you have advantage on the attack (maximum of once per turn).
              </p>
            </li>
          </ul>

          <h3 class="inline-h">            
            <a href="spells.html#acid-arrows">Acid Arrows</a>
            <img class="spell-icon" src="img/icons/transmutation.png"> <p class="spell-level">2nd</p>
          </h3>
          <p class="book sofw">(SoFW)</p>

          <ul>
            <li>
              <p class="short-desc">
                You touch a quiver containing arrows or bolts. When a target is hit by an attack using a piece of ammunition drawn from the quiver, the target takes an extra 1d4 acid damage. The spell’s magic ends on the piece of ammunition when it hits or misses.
              </p>
            </li>
          </ul> 
            
        </div>
      </div>

      <div class="w3-card-4 w3-margin card">
        <div id="races" class="w3-container">
          <h2>
            <b>Races</b>
            <!-- <a href=""><img class="link-icon" src="img/icons/races.png"></a> -->
            <img class="misc-icon" src="img/icons/races.png">
          </h2>

          <h3 class="inline-h">            
            <a href="https://www.dndbeyond.com/races/742629-barbarus-aarakocra" target="_blank">Barbarus Aarakocra</a> 
          </h3>
          <p class="book kryat">(Kryat)</p>

          <ul>
            <li>
              <p class="short-desc">
                Aarakocra from the moon of Barbarus have evolved to breath dense air of Barbarus, as such they require special accommodation to breath on Terra or any of the other moons.
              </p>
            </li>
          </ul>

          <h3 class="inline-h">            
            <a href="https://www.dndbeyond.com/races/743174-chiroptera" target="_blank">Chiroptera</a>
          </h3>
          <p class="book kryat">(Kryat)</p>

          <ul>
            <li>
              <p class="short-desc">
                Chiroptera are a bat-like race that evolved on the moon of Barbarus. The Chiroptera were the first sentient race to evolve on Barbarus.
              </p>
            </li>
          </ul>

          <h3 class="inline-h">            
            <a href="https://www.dndbeyond.com/races/742585-ratkin" target="_blank">Ratkin</a>
          </h3>
          <p class="book kryat">(Kryat)</p>

          <ul>
            <li>
              <p class="short-desc">
                Ratkin are the dominant race of the Underdark or at least that is what they believe. Ratkin communities often inhabit caverns a short distance from Drow or Gnome cites.
              </p>
            </li>
          </ul>
          
        </div>
      </div>

      <div class="w3-card-4 w3-margin card">
        <div id="feats" class="w3-container">
          <h2><b>Feats</b> <a href="feats.html"><img class="link-icon" src="img/icons/anvil.png"></a></h2>

          <h3 class="inline-h">            
            <a href="feats.html#arcane-shot-adept">Arcane Shot Adept</a> 
          </h3>
          <p class="book sofw">(SoFW)</p>

          <ul>
            <li>
              <p class="short-desc">
                You have training to unleash special magical effects with shortbow and longbow attacks.
              </p>
            </li>
          </ul>

        </div>
      </div>

      <div class="w3-card-4 w3-margin card">
        <div id="invocations" class="w3-container">
          <h2><b>Warlock Invocations</b> <a href="invocations.html"><img class="link-icon" src="img/icons/warlock-eye.svg"></a></h2>

          <h3 class="inline-h">            
            <a href="invocations.html#eldritch-resolve">Eldritch Resolve</a> 
          </h3>
          <p class="book sofw">(SoFW)</p>

          <ul>
            <li>
              <p class="short-desc">
                Whenever you cast a spell using a warlcok pact spell slot, you can uses your bonus action to draw upon your patron's power to bolster your resolve.
              </p>
            </li>
          </ul>

        </div>
      </div>

    </div>

<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ -->

    <div class="w3-col l4">      
      <div class="w3-card w3-margin-left w3-margin-top">
        <div class="w3-container card">

          <h3 id="glossary">Site Inventory</h3>
          <ul>
            <li class="sidebar"><a href="#published">Published Subclasses</a></li>
            <li class="sidebar"><a href="drafts.html">Draft Subclasses</a></li>
            <li class="sidebar"><a href="spells.html">Spell Book</a></li>
            <li class="sidebar"><a href="#races">Races</a></li>
            <li class="sidebar"><a href="feats.html">Feats</a></li>
            <li class="sidebar"><a href="invocations.html">Warlock Invocations</a></li>
          </ul>
          
        </div>
      </div>
    </div>

    <div class="w3-col l4">
      <div class="w3-card w3-margin-left w3-margin-top">
        <div class="w3-container card">

          <h3 id="key">Key</h3>
          <ul>
            <li class="sidebar">
              <p class="sofw sidebar">SoFW</p> <p class="sidebar">= The Scrolls of Forgotten Warriors</p>
            </li>
            <li class="sidebar">
              <p class="eots sidebar">EotS</p> <p class="sidebar">= Echos of the Spellplague</p>
            </li>
            <li class="sidebar">
              <p class="odvaskar sidebar">AoFL+</p> <p class="sidebar">= Addition to Archive of Forgotten Lore by <a href="https://www.patreon.com/Odvaskar" target="_blank">Odvaskar</a></p>
            </li>
            <li class="sidebar">
              <p class="kryat sidebar">Kryat</p> <p class="sidebar">= Kryat (Homebrew Setting)</p>
            </li>
          </ul>
          
        </div>
      </div>
    </div>

    <div class="w3-col l4 img-card">
          <a href="index.html"><img src="img/duder-logo.png" class="side-img"></a>
    </div>

  </body>

</html>
