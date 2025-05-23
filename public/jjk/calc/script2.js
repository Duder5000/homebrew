function getHitDie(className) {
    let hitDie;

    if(className == "shinobi"){
        className = "scout";
    }

    if(className == "jug"){
        hitDie = 12;
    }else if(className == "scout"){
        hitDie = 8;
    }else if(className == "strat"){
        hitDie = 6;
    }else if(className == "shikigami"){
        hitDie = 8;
    }else if(className == "support"){
        hitDie = 6;
    }else if(className == "warrior"){
        hitDie = 10;
    }

    return hitDie;
}

function baseCE(className, level, ceMod, pb, classNum, firstClass) {
    let ceVal;

    if(className == "shinobi"){
        className = "scout";
    }

    if(classNum <= 1){
        if (className === "jug") {
            ceVal = level;
        } else if (className === "scout") {
            ceVal = level + ceMod;
        } else if (className === "strat") {
            ceVal = (level * 2) + ceMod;
        } else if (className === "shikigami") {
            ceVal = level + ceMod;
        } else if (className === "support") {
            ceVal = level + ceMod + pb;
        } else if (className === "warrior") {
            ceVal = level + ceMod;
        } else {
            ceVal = level; 
        }
    }

    if(classNum > 1){
        ceVal = 9999;

        if(className === "jug"){
            ceVal = level;
        }else if (className === "scout") {
            if (firstClass === "jug") {
                ceVal = level + ceMod;
            } else if (firstClass === "shikigami" || firstClass === "warrior") {
                ceVal = level;
            } else if (firstClass === "strat") {
                ceVal = level;
            }else if (firstClass === "support") {
                ceVal = level;
            }
        } else if (className === "strat") {
            if (firstClass === "jug") {
                ceVal = (level * 2) + ceMod;
            } else if (firstClass === "scout" || firstClass === "shikigami" || firstClass === "warrior") {
                ceVal = (level * 2);
            } else if (firstClass === "support") {
                ceVal = (level * 2) + ceMod;
            } 
        } else if (className === "shikigami") {
            if (firstClass === "jug") {
                ceVal = level + ceMod;
            } else if (firstClass === "scout" || firstClass === "warrior") {
                ceVal = level;
            } else if (firstClass === "strat") {
                ceVal = level;
            }else if (firstClass === "support") {
                ceVal = level + pb;
            }
        } else if (className === "support") {
            if (firstClass === "jug") {
                ceVal = level + ceMod + pb;
            } else if (firstClass === "scout" || firstClass === "shikigami" || firstClass === "warrior") {
                ceVal = level + pb;
            } else if (firstClass === "strat") {
                ceVal = (level * 2);
            }
        } else if (className === "warrior") {
            if (firstClass === "jug") {
                ceVal = level + ceMod;
            } else if (firstClass === "scout" || firstClass === "shikigami") {
                ceVal = level;
            } else if (firstClass === "strat") {
                ceVal = (level * 2);
            }else if (firstClass === "support") {
                ceVal = level + pb;
            }
        } else {
            ceVal = 0; 
        }
    }

    return ceVal;
}

function calculateHP() {
    const level = parseInt(document.getElementById('level').value) || 1;
    const conMod  = parseInt(document.getElementById('conMod').value) || 0;
    // const hitDie = parseInt(document.getElementById('hitDie').value) || 8;
    const className = document.getElementById('class').value;
    const hasTough = document.getElementById('tough').checked;

    const hpPerLevel = parseInt(document.getElementById('hpPerLevel').value) || 0;
    const hpFlat = parseInt(document.getElementById('hpFlat').value) || 0;

    const has2ndClass = document.getElementById('secondClassCheck').checked;
    const level2 = parseInt(document.getElementById('level2').value) || 0;
    const className2 = document.getElementById('class2').value;

    hitDie = getHitDie(className);

    hitDie2 = 0;
    additionalHP2 = 0;
    if(has2ndClass){
        hitDie2 = getHitDie(className2);
        additionalHP2 = level2 * ((hitDie2 / 2) + 1 + conMod); // Average roll for additional levels
    }

    let baseHP = hitDie + conMod; // Initial HP at level 1
    let additionalHP = (level - 1) * ((hitDie / 2) + 1 + conMod); // Average roll for additional levels

    if (hasTough) {
        baseHP += 2 * level; // Tough feat adds 2 HP per level
    }

    const totalHP = baseHP + additionalHP + (hpPerLevel*level) + hpFlat + additionalHP2;

    document.getElementById('result1').innerText = "HP: " + totalHP;
}

function calculateCE() {
    const level = parseInt(document.getElementById('level').value) || 1;
    const className = document.getElementById('class').value;
    const ceMod = parseInt(document.getElementById('ceMod').value) || 0;
    const hasBottomless = document.getElementById('bottomless').checked;
    const hasImmense = document.getElementById('immense').checked;

    const cePerLevel = parseInt(document.getElementById('cePerLevel').value) || 0;
    const ceFlat = parseInt(document.getElementById('ceFlat').value) || 0;

    const has2ndClass = document.getElementById('secondClassCheck').checked;
    const level2 = parseInt(document.getElementById('level2').value) || 0;
    const className2 = document.getElementById('class2').value;

    const pb = Math.ceil(1 + (level/4));

    if (className != "na") {

        ceVal = baseCE(className, level, ceMod, pb, 1, "na");

        ceVal = ceVal + (cePerLevel*level) + ceFlat;

        ceVal2 = 0;
        if(has2ndClass){
            ceVal2 = baseCE(className2, level2, ceMod, pb, 2, className);
        }

        if(hasImmense){
            ceVal += ceMod;
        }

        if(hasBottomless){
            ceVal += level * 2;
        }

        drawText = "CE: " +  (ceVal + ceVal2);

        if(has2ndClass){
            drawText += " (" + ceVal + ", " + ceVal2 + ")";
        }

        // document.getElementById('result2').innerText = "CE: " +  (ceVal + ceVal2) + " (" + ceVal + ", " + ceVal2 + ")";
        document.getElementById('result2').innerText = drawText;
    } else {
        document.getElementById('result2').innerText = "CE: n/a";
    }
}

// Attach event listeners to update HP calculation when inputs change
document.getElementById('level').addEventListener('input', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('level2').addEventListener('input', () => {
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
document.getElementById('class').addEventListener('change', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('class2').addEventListener('change', () => {
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
document.getElementById('hpPerLevel').addEventListener('change', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('hpFlat').addEventListener('change', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('cePerLevel').addEventListener('change', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('ceFlat').addEventListener('change', () => {
    calculateHP();
    calculateCE();
});
document.getElementById('secondClassCheck').addEventListener('change', () => {
    calculateHP();
    calculateCE();
});

// ---------------------------

secondClassDiv.style.display = 'none';

// Initial calculation to display default value
calculateHP();
calculateCE();

document.getElementById('secondClassCheck').addEventListener('change', function() {
    var secondClassDiv = document.getElementById('secondClassDiv');
    if (this.checked) {
        secondClassDiv.style.display = 'block';
    } else {
        secondClassDiv.style.display = 'none';
    }
});