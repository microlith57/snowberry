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
    return {
        [materialKey] = {
            fieldType = "snowberry:tileset"
        }
    }
end

return fakeTilesHelper