-- snowberry header
local snowberry_orig_require = require
local require = function(name)
    return snowberry_orig_require("#Snowberry.LoennPluginLoader").EverestRequire(name)
end
-- end snowberry header

local drawableSprite = require("structs.drawable_sprite")

local texName = "snowberry/1x1"

local drawableRectangle = {}

-- for now we just Don't
function drawableRectangle.fromRectangle(mode, x, y, width, height, color, secondaryColor)
    local sprite = drawableSprite.fromTexture("snowberry/1x1", { x = x, y = y, color = color })
    sprite:setScale(width, height)
    sprite:setJustification(0, 0)
    return sprite
end

return drawableRectangle