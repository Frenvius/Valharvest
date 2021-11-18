// const foodJson = require('../JotunnModStub/Configs/newFoodsConfig.json');
const foodJson = require('../JotunnModStub/Configs/BoneAppetitBalance.json');
const ingredientsJson = require('./ingredients.json');

let count = 1
for (let food in foodJson) {
    count++;
    // if (foodJson[food].name == "Deer stew" || foodJson[food].name == "Minced Meat Sauce" || foodJson[food].name == "Carrot soup" || foodJson[food].name == "Fish wraps" || foodJson[food].name == "Bread") {
        let health = 0;
        let stam = 0;
        // console.log(foodJson[food].name);
        for (let req in foodJson[food].requirements) {
            const reqName = foodJson[food].requirements[req].Item;
            const reqAmount = foodJson[food].requirements[req].Amount;
            const ingHealth = ingredientsJson[reqName].health;
            const ingStam = ingredientsJson[reqName].stamina;
            const ingType = ingredientsJson[reqName].type;
            const healthMult = ingredientsJson[reqName].healthMult;
            const stamMult = ingredientsJson[reqName].staminaMult;
         
            health += ingHealth ? (ingHealth * reqAmount) * healthMult : 0;
            stam += (ingStam * reqAmount) * stamMult;

            if (foodJson[food].requirements.length == 1) {
                if (reqName == "rice") {
                    health -= 10;
                    stam += 15;
                    if (reqAmount > 2) {
                        health -= (8.5 * reqAmount - 2);
                        stam -= (4.5 * reqAmount - 2);
                    }
                }   
            }
        }

        // console.log(foodJson[food].name.substring(0, 6) + ":	");
        // console.log(round5(health) + "	" + round5(stam));
        console.log(foodJson[food].name.substring(0, 6) + ":	"+round5(health) + "	" + round5(stam));
    // }
};

function round5(x){
    return Math.ceil(x/5)*5;
}
