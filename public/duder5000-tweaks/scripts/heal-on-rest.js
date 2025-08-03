Hooks.on("pf2e.restForTheNight", async (actor) => {
  const current = actor.system.attributes.hp.value;
  const max = actor.system.attributes.hp.max;

  if (current < max) {
    await actor.update({ "system.attributes.hp.value": max });

    await new Promise(resolve => setTimeout(resolve, 100));

    const lastMessage = [...game.messages]
      .reverse()
      .find(msg => msg.speaker?.actor === actor.id);

    if (
      lastMessage &&
      (lastMessage.isOwner || game.user.isGM) &&
      lastMessage.content?.includes("awakens well-rested")
    ) {
      await lastMessage.delete();
    }

    ChatMessage.create({
      speaker: ChatMessage.getSpeaker({ actor }),
      content: `<p><strong>${actor.name}</strong> fully recovers to <strong>${max} HP</strong> after resting for the night.</p>`
    });
  }
});