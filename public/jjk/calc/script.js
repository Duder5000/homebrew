function calculateHP() {
    const level = parseInt(document.getElementById('level').value) || 1;
    const conMod = parseInt(document.getElementById('conMod').value) || 0;
    const hitDie = parseInt(document.getElementById('hitDie').value) || 8;
    const hasTough = document.getElementById('tough').checked;

    let baseHP = hitDie + conMod; // Initial HP at level 1
    let additionalHP = (level - 1) * ((hitDie / 2) + 1 + conMod); // Average roll for additional levels

    if (hasTough) {
        baseHP += 2 * level; // Tough feat adds 2 HP per level
    }

    const totalHP = baseHP + additionalHP;

    document.getElementById('result1').innerText = "HP: " + totalHP;
}

function calculateCE() {
    const level = parseInt(document.getElementById('level').value) || 1;
    const className = document.getElementById('class').value;
    const ceMod = parseInt(document.getElementById('ceMod').value) || 0;
    const hasBottomless = document.getElementById('bottomless').checked;
    const hasImmense = document.getElementById('immense').checked;

    const pb = Math.ceil(1 + (level/4));

    if (className != "na") {
        ceVal = 0;

        if(className == "jug"){
            tempName = "Juggernaut";
            ceVal = level;
        }else if(className == "scout"){
            tempName = "Scout";
            ceVal = level + ceMod;
        }else if(className == "strat"){
            tempName = "Strategist";
            ceVal = (level*2) + ceMod;
        }else if(className == "shikigami"){
            tempName = "Shikigami User";
            ceVal = level + ceMod;
        }else if(className == "support"){
            tempName = "Support";
            ceVal = level + ceMod + pb;
        }else if(className == "warrior"){
            tempName = "Warrior";
            ceVal = level + ceMod;
        }
        document.getElementById('result2').innerText = "CE: "+  ceVal;
    } else {
        document.getElementById('result2').innerText = "CE: n/a";
    }
}

// Attach event listeners to update HP calculation when inputs change
document.getElementById('level').addEventListener('input', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('conMod').addEventListener('input', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('ceMod').addEventListener('input', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('hitDie').addEventListener('change', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('class').addEventListener('change', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('tough').addEventListener('change', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('bottomless').addEventListener('change', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('immense').addEventListener('change', () => {
    calculateHP();
    calculateCE();
});

// Initial calculation to display default value
calculateHP();
calculateCE();