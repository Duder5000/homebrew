Hooks.on("chatMessage", (chatLog, messageText, chatData) => {
  const command = "/rest";

  if (messageText.toLowerCase().startsWith(command)) {
    
    // 1. Figure out which actors to heal
    let actorsToRest = [];
    
    // First, check for selected tokens on the canvas
    if (canvas.tokens.controlled.length > 0) {
      actorsToRest = canvas.tokens.controlled.map(token => token.actor).filter(actor => actor !== null);
    } 
    // Fallback: Check if the user has a default character assigned
    else if (game.user.character) {
      actorsToRest = [game.user.character];
    }

    // 2. Abort if we couldn't find anyone
    if (actorsToRest.length === 0) {
      ui.notifications.warn("Please select a token or ensure you have a character assigned to your player.");
      return false; // Still return false so the raw command doesn't print
    }

    // 3. Process each actor
    actorsToRest.forEach(actor => {
      // NOTE: This data path is for systems like D&D 5e. 
      // Other systems might store HP differently.
      const maxHp = actor.system?.attributes?.hp?.max;
      
      if (maxHp !== undefined) {
        // Update the actor's current HP to match their max HP
        actor.update({ "system.attributes.hp.value": maxHp });
        
        // Output a narrative message to the chat
        ChatMessage.create({
          speaker: ChatMessage.getSpeaker({ actor: actor }),
          content: `<em>${actor.name} rests for the night, recovering all their hit points.</em>`
        });
      } else {
        ui.notifications.error(`Could not find Max HP data for ${actor.name}.`);
      }
    });

    // 4. Cancel the default chat message
    return false;
  }
  
  return true; 
});