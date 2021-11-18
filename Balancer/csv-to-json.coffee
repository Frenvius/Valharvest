{tab}"{f12}": {lb}
{tab}{tab}"name": "{f1}",
{tab}{tab}"description": {(f2)==""?"null":"\""+f2+"\""},
{tab}{tab}"weight": {f3.replace(',', '.')},
{tab}{tab}"variants": {(f4)==""?"null":f4},
{tab}{tab}"food": {(f5)==""?"null":f5},
{tab}{tab}"foodStamina": {(f6)==""?"null":f6},
{tab}{tab}"foodBurnTime": {f7.toNumber()},
{tab}{tab}"foodRegen": {(f8)==""?"null":f8},
{tab}{tab}"maxStackSize": {(f9)==""?"null":f9},
{tab}{tab}"color": {(f10)==""?"null":"\""+f10+"\""},
{tab}{tab}"amount": {(f11)==""?"null":f11},
{tab}{tab}"craftingStation": "{f13}",
{tab}{tab}"requirements": [
{tab}{tab}{tab}{lb}"Item": "{f15}", "Amount": {f14}{rb},
{tab}{tab}{tab}{if((f16.length) != 0) decodeURI('%7B')+"\"Item\"\: \""+f17+"\", \"Amount\"\: "+f16+decodeURI('%7D')+","}
{tab}{tab}{tab}{if((f18.length) != 0) decodeURI('%7B')+"\"Item\"\: \""+f19+"\", \"Amount\"\: "+f18+decodeURI('%7D')+","}
{tab}{tab}{tab}{if((f20.length) != 0) decodeURI('%7B')+"\"Item\"\: \""+f21+"\", \"Amount\"\: "+f20+decodeURI('%7D')}
{tab}{tab}]
{tab}{rb}