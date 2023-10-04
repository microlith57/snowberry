-- snowberry header
local snowberry_orig_require = require
local require = function(name)
    return snowberry_orig_require("#Snowberry.LoennPluginLoader").EverestRequire(name)
end
-- end snowberry header

local utils = require("utils")

local drawableRectangle = {}

local drawableRectangleMt = {}
drawableRectangleMt.__index = {}

drawableRectangle.tintingPixelTexture = "snowberry/1x1"

-- for now we just Don't
function drawableRectangleMt.__index:getDrawableSprite()
    return self
end

function drawableRectangle.fromRectangle(mode, x, y, width, height, color, secondaryColor)
    local rectangle = {
        _type = "drawableRectangle"
    }

    rectangle.mode = mode

    if type(x) == "table" then
        rectangle.x = x.x or x[1]
        rectangle.y = x.y or x[2]

        rectangle.width = x.width or x[3]
        rectangle.height = x.height or x[4]

        rectangle.color = utils.getColor(y)
        rectangle.secondaryColor = utils.getColor(width)
    else
        rectangle.x = x
        rectangle.y = y

        rectangle.width = width
        rectangle.height = height

        rectangle.color = utils.getColor(color)
        rectangle.secondaryColor = utils.getColor(secondaryColor)
    end

    return setmetatable(rectangle, drawableRectangleMt)
end

return drawableRectangle