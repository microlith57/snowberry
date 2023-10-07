-- here we do something called "cheating"
local sb = require("#Snowberry.LoennPluginLoader")

local fakeTilesHelper = {}

function fakeTilesHelper.getEntitySpriteFunction(materialKey, blendKey, layer, color, x, y)
    layer = layer or "tilesFg"
    
    return function(room, entity)
        local tx = x or entity.x or 0
        local ty = y or entity.y or 0
        local key = entity[materialKey]
        
        return {
            _type = "tileGrid",
            color = color,
            x = tx,
            y = ty,
            matrix = sb.Autotile(layer, key, entity.width, entity.height)
        }
    end
end

function fakeTilesHelper.getFieldInformation(materialKey, layer)
    return function()
        return {
            [materialKey] = {
                fieldType = "snowberry:tileset"
            }
        }
    end
end

-- TODO: when loenn dropdowns are implemented, make this actually look at the map's tilesets (?)
-- ...but we don't dynamically look at field info, so it wouldn't work (?)
function fakeTilesHelper.getTilesOptions(layer)
    return {}
end

return fakeTilesHelper